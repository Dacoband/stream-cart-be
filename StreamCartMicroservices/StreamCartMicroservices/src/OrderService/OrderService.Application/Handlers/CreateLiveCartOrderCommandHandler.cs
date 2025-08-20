using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Common.Models;
using Shared.Messaging.Event.OrderEvents;
using Shared.Messaging.Event.LivestreamEvents;
using MassTransit;

namespace OrderService.Application.Handlers
{
    public class CreateLiveCartOrderCommandHandler : IRequestHandler<CreateLiveCartOrderCommand, ApiResponse<LivestreamOrderResult>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IAdressServiceClient _addressServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IShopVoucherClientService _shopVoucherClientService;
        private readonly ILogger<CreateLiveCartOrderCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAccountServiceClient _accountServiceClient;

        public CreateLiveCartOrderCommandHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            IAdressServiceClient addressServiceClient,
            IShopServiceClient shopServiceClient,
            IShopVoucherClientService shopVoucherClientService,
            ILogger<CreateLiveCartOrderCommandHandler> logger,
            IPublishEndpoint publishEndpoint,
            IAccountServiceClient accountServiceClient)
        {
            _orderRepository = orderRepository;
            _productServiceClient = productServiceClient;
            _addressServiceClient = addressServiceClient;
            _shopServiceClient = shopServiceClient;
            _shopVoucherClientService = shopVoucherClientService;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<ApiResponse<LivestreamOrderResult>> Handle(CreateLiveCartOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🛒 Processing live cart order for user {UserId} in livestream {LivestreamId}",
                    request.UserId, request.LivestreamId);

                // 1. Validate và lấy thông tin delivery address
                var deliveryAddress = await _addressServiceClient.GetCustomerAddress(request.DeliveryAddressId.ToString(), "system");
                if (deliveryAddress == null)
                {
                    return ApiResponse<LivestreamOrderResult>.ErrorResult("Địa chỉ giao hàng không hợp lệ");
                }

                // 2. Group cart items by shop
                var shopGroups = request.CartItems.GroupBy(item => item.ShopId);
                var orders = new List<Orders>();
                var totalAmount = 0m;
                var totalItems = 0;

                foreach (var shopGroup in shopGroups)
                {
                    var shopId = shopGroup.Key;
                    var shopItems = shopGroup.ToList();

                    // Validate shop exists
                    var shopInfo = await _shopServiceClient.GetShopByIdAsync(shopId);
                    if (shopInfo == null)
                    {
                        return ApiResponse<LivestreamOrderResult>.ErrorResult($"Shop {shopId} không tồn tại");
                    }

                    var shopAddress = await _shopServiceClient.GetShopAddressAsync(shopId);
                    if (shopAddress == null)
                    {
                        return ApiResponse<LivestreamOrderResult>.ErrorResult($"Không tìm thấy địa chỉ shop {shopId}");
                    }

                    var order = await CreateOrderForShop(
                        request.UserId,
                        request.LivestreamId,
                        shopInfo,
                        shopAddress,
                        shopItems,
                        deliveryAddress,
                        request.PaymentMethod,
                        request.CustomerNotes,
                        request.VoucherCode,
                        request.ShippingProviderId,
                        request.ShippingFee,
                        request.ExpectedDeliveryDay,
                        request.CreatedFromCommentId);

                    if (order == null)
                    {
                        return ApiResponse<LivestreamOrderResult>.ErrorResult($"Không thể tạo đơn hàng cho shop {shopInfo.ShopName}");
                    }

                    orders.Add(order);
                    totalAmount += order.FinalAmount;
                    totalItems += order.Items?.Sum(i => i.Quantity) ?? 0;
                }

                // 3. Save all orders
                foreach (var order in orders)
                {
                    await _orderRepository.InsertAsync(order);
                }

                // 4. Publish events for each order
                foreach (var order in orders)
                {
                    await PublishOrderEventsAsync(order, request.PaymentMethod, request.UserId);
                }

                // 5. Publish LIVESTREAM-SPECIFIC EVENT for real-time updates
                await PublishLivestreamOrderStatsUpdateAsync(request.LivestreamId, orders);

                // 6. Get first order for result (hoặc tạo combined result nếu cần)
                var mainOrder = orders.First();

                var result = new LivestreamOrderResult
                {
                    OrderId = mainOrder.Id,
                    OrderCode = mainOrder.OrderCode,
                    TotalAmount = totalAmount,
                    CreatedAt = mainOrder.OrderDate,
                    ItemCount = totalItems,
                    LivestreamId = request.LivestreamId
                };

                _logger.LogInformation("✅ Successfully created {OrderCount} orders from live cart for livestream {LivestreamId}",
                    orders.Count, request.LivestreamId);

                return ApiResponse<LivestreamOrderResult>.SuccessResult(result, "🎉 Đặt hàng từ livestream thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating live cart order");
                return ApiResponse<LivestreamOrderResult>.ErrorResult($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private async Task<Orders?> CreateOrderForShop(
            Guid userId,
            Guid livestreamId,
            ShopDto shopInfo,
            AddressOfShop shopAddress,
            List<LiveCartItemDto> shopItems,
            AdressDto deliveryAddress,
            string paymentMethod,
            string? customerNotes,
            string? voucherCode = null,
            Guid? shippingProviderId = null,
            decimal? shippingFee = null,
            DateTime? expectedDeliveryDay = null,
            Guid? createdFromCommentId = null)
        {
            try
            {
                var order = new Orders(
                    accountId: userId,
                    shopId: shopInfo.Id,
                    toName: deliveryAddress.RecipientName,
                    toPhone: deliveryAddress.PhoneNumber,
                    toAddress: deliveryAddress.Street,
                    toWard: deliveryAddress.Ward,
                    toDistrict: deliveryAddress.District,
                    toProvince: deliveryAddress.City,
                    toPostalCode: deliveryAddress.PostalCode,
                    fromAddress: shopAddress.Street,
                    fromWard: shopAddress.Ward,
                    fromDistrict: shopAddress.District,
                    fromProvince: shopAddress.City,
                    fromPostalCode: shopAddress.PostalCode,
                    fromShop: shopAddress.RecipientName,
                    fromPhone: shopAddress.PhoneNumber,
                    shippingProviderId: shippingProviderId ?? Guid.Empty,
                    customerNotes: customerNotes ?? "",
                    livestreamId: livestreamId,
                    createdFromCommentId: createdFromCommentId,
                    paymentMethod: paymentMethod); // ✅ ADDED: Pass payment method

                order.SetCreator(userId.ToString());

                if (shippingFee.HasValue && shippingFee.Value > 0)
                {
                    order.SetShippingFee(shippingFee.Value, userId.ToString());
                }


                if (expectedDeliveryDay.HasValue)
                {
                }

                var orderItems = new List<OrderItem>();

                // Process each item
                foreach (var cartItem in shopItems)
                {
                    // Get product info
                    var product = await _productServiceClient.GetProductByIdAsync(cartItem.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("Product {ProductId} not found", cartItem.ProductId);
                        continue;
                    }

                    // ✅ ADDED: Get variant name if variant exists
                    string variantName = string.Empty;
                    if (cartItem.VariantId.HasValue)
                    {
                        var variant = await _productServiceClient.GetVariantByIdAsync(cartItem.VariantId.Value);
                        if (variant != null)
                        {
                           // variantName = variant.Name ?? string.Empty;
                        }
                    }

                    // Check stock
                    if (product.StockQuantity < cartItem.Quantity)
                    {
                        _logger.LogWarning("Not enough stock for product {ProductId}: requested {Requested}, available {Available}",
                            cartItem.ProductId, cartItem.Quantity, product.StockQuantity);
                        continue;
                    }
                    var unitPrice = cartItem.UnitPrice ?? product.FinalPrice;
                    var discountAmount = cartItem.DiscountAmount ?? 0m;

                    // Create order item
                    var orderItem = new OrderItem(
                        orderId: order.Id,
                        productId: cartItem.ProductId,
                        quantity: cartItem.Quantity,
                        unitPrice: unitPrice,
                        discountAmount: discountAmount,
                        notes: "",
                        variantId: cartItem.VariantId);

                    orderItems.Add(orderItem);
                }

                if (!orderItems.Any())
                {
                    _logger.LogWarning("No valid items for shop {ShopId}", shopInfo.Id);
                    return null;
                }

                // Add items to order
                order.AddItems(orderItems);

                if (!string.IsNullOrEmpty(voucherCode))
                {
                    await ApplyVoucherToOrderAsync(order, voucherCode, shopInfo.Id, userId.ToString());
                }

                if (paymentMethod != "COD")
                {
                    order.OrderStatus = OrderStatus.Pending;
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for shop {ShopId}", shopInfo.Id);
                return null;
            }
        }

        private async Task ApplyVoucherToOrderAsync(Orders order, string voucherCode, Guid shopId, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("🎫 Applying voucher {VoucherCode} to order {OrderId} for shop {ShopId}",
                    voucherCode, order.Id, shopId);

                // Validate voucher first
                var validation = await _shopVoucherClientService.ValidateVoucherAsync(voucherCode, order.TotalPrice, shopId);
                if (validation == null || !validation.IsValid)
                {
                    _logger.LogWarning("❌ Voucher validation failed: {Message}", validation?.Message ?? "Unknown error");
                    return;
                }

                var accessToken = "";
                var applied = await _shopVoucherClientService.ApplyVoucherAsync(voucherCode, order.Id, order.TotalPrice, shopId, accessToken);

                if (applied != null && applied.IsApplied)
                {
                    // Apply discount to order
                    order.ApplyDiscount(applied.DiscountAmount, modifiedBy);
                    order.VoucherCode = applied.VoucherCode;

                    _logger.LogInformation("✅ Voucher applied successfully! Code: {Code}, Discount: {Discount}đ",
                        applied.VoucherCode, applied.DiscountAmount);
                }
                else
                {
                    _logger.LogWarning("❌ Voucher application failed: {Message}", applied?.Message ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error applying voucher {VoucherCode} to order {OrderId}", voucherCode, order.Id);
            }
        }

        private async Task PublishOrderEventsAsync(Orders order, string paymentMethod, Guid userId)
        {
            try
            {
                var orderItemEvent = order.Items.Select(x => new OrderItemInEvent
                {
                    ProductId = x.ProductId.ToString(),
                    VariantId = x.VariantId?.ToString(),
                    Quantity = x.Quantity,
                }).ToList();

                if (paymentMethod == "COD")
                {
                    var shopAccounts = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                    if (shopAccounts != null && shopAccounts.Any())
                    {
                        var @event = new OrderCreatedOrUpdatedEvent
                        {
                            OrderCode = order.OrderCode,
                            Message = "đã được tạo thành công từ livestream. Vui lòng chuẩn bị đơn hàng",
                            UserId = shopAccounts.Select(x => x.Id.ToString()).ToList(),
                            OrderStatus = "Pending",
                            OrderItems = orderItemEvent,
                            ShopId = order.ShopId.ToString(),
                            CreatedBy = order.CreatedBy,
                        };

                        await _publishEndpoint.Publish(@event);
                    }
                }
                else
                {
                    var @event = new OrderCreatedOrUpdatedEvent
                    {
                        OrderCode = order.OrderCode,
                        Message = "đã được tạo thành công từ livestream",
                        UserId = new List<string> { userId.ToString() },
                        OrderStatus = "Waiting",
                        OrderItems = orderItemEvent,
                        ShopId = order.ShopId.ToString(),
                        CreatedBy = order.CreatedBy,
                    };

                    await _publishEndpoint.Publish(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing order events for order {OrderId}", order.Id);
            }
        }

        private async Task PublishLivestreamOrderStatsUpdateAsync(Guid livestreamId, List<Orders> newOrders)
        {
            try
            {
                var totalNewOrders = newOrders.Count;
                var totalNewRevenue = newOrders.Sum(o => o.FinalAmount);
                var totalNewItems = newOrders.Sum(o => o.Items?.Sum(i => i.Quantity) ?? 0);

                var livestreamStatsEvent = new LivestreamOrderStatsUpdatedEvent
                {
                    LivestreamId = livestreamId,
                    NewOrderCount = totalNewOrders,
                    NewRevenue = totalNewRevenue,
                    NewItemCount = totalNewItems,
                    OrderIds = newOrders.Select(o => o.Id).ToList(),
                    Timestamp = DateTime.UtcNow,
                    ProductsSold = newOrders
                        .SelectMany(o => o.Items)
                        .GroupBy(i => i.ProductId)
                        .Select(g => new LivestreamProductSalesInfo
                        {
                            ProductId = g.Key,
                            QuantitySold = g.Sum(i => i.Quantity),
                            Revenue = g.Sum(i => i.UnitPrice * i.Quantity)
                        }).ToList()
                };

                await _publishEndpoint.Publish(livestreamStatsEvent);

                _logger.LogInformation("📊 Published livestream stats update for {LivestreamId}: {OrderCount} orders, {Revenue:N0}đ",
                    livestreamId, totalNewOrders, totalNewRevenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error publishing livestream stats update for {LivestreamId}", livestreamId);
            }
        }
    }
}