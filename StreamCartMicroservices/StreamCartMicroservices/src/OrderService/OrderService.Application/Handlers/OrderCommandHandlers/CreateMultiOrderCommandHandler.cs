    using MassTransit;
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
    using Shared.Common.Services.User;
    using Shared.Messaging.Event.OrderEvents;
    using System;
    using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

namespace OrderService.Application.Handlers.OrderCommandHandlers
    {
        public class CreateMultiOrderCommandHandler : IRequestHandler<CreateMultiOrderCommand, ApiResponse<List<OrderDto>>>
        {
            private readonly IOrderRepository _orderRepository;
            private readonly IProductServiceClient _productServiceClient;
            private readonly IShopServiceClient _shopServiceClient;
            private readonly ILogger<CreateMultiOrderCommandHandler> _logger;
            private readonly IPublishEndpoint _publishEndpoint;
            private readonly IAdressServiceClient _addressServiceClient;
            private readonly IMembershipServiceClient _membershipServiceClient;
            private readonly IShopVoucherClientService _shopVoucherClientService;
            private readonly IAccountServiceClient _accountServiceClient;
            private readonly ICurrentUserService _currentUserService;
            private readonly IOrderNotificationQueue _orderNotificationQueue;
            private readonly ILivestreamServiceClient _livestreamServiceClient; // ✅ ADDED

        public CreateMultiOrderCommandHandler(
                IOrderRepository orderRepository,
                IProductServiceClient productServiceClient,
                IShopServiceClient shopServiceClient,
                ILogger<CreateMultiOrderCommandHandler> logger,
                IPublishEndpoint publishEndpoint, IAdressServiceClient adressServiceClient, IMembershipServiceClient membershipServiceClient, IShopVoucherClientService shopVoucherClientService, IAccountServiceClient accountServiceClient, ICurrentUserService currentUserService, IOrderNotificationQueue notificationQueue, ILivestreamServiceClient livestreamServiceClient)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _shopServiceClient = shopServiceClient ?? throw new ArgumentNullException(nameof(shopServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
            _addressServiceClient = adressServiceClient;
            _membershipServiceClient = membershipServiceClient;
            _shopVoucherClientService = shopVoucherClientService;
            _accountServiceClient = accountServiceClient;
            _currentUserService = currentUserService;
            _orderNotificationQueue = notificationQueue;
            _livestreamServiceClient = livestreamServiceClient ?? throw new ArgumentNullException(nameof(livestreamServiceClient));
        }

        public async Task<ApiResponse<List<OrderDto>>> Handle(CreateMultiOrderCommand request, CancellationToken cancellationToken)
        {
            var response = new ApiResponse<List<OrderDto>>
            {
                Success = true,
                Message = "Tạo đơn hàng thành công",
                Data = new List<OrderDto>()
            };

            var accessToken = _currentUserService.GetAccessToken();
            _logger.LogInformation("Creating multiple orders for account {AccountId}", request.AccountId);
            var currentFlashSales = await _productServiceClient.GetCurrentFlashSalesAsync();
            var flashIndex = (currentFlashSales ?? new List<FlashSaleDetailDTO>())
                .GroupBy(fs => (fs.ProductId, fs.VariantId ?? Guid.Empty))
                .ToDictionary(g => g.Key, g => g.First().Id);

            // 🔹 NEW: cache tránh gọi GET trùng
            var productCache = new Dictionary<Guid, ProductDto>();
            var variantCache = new Dictionary<Guid, VariantDto>();
            foreach (var shopOrder in request.OrdersByShop)
            {
                try
                {
                    // Lấy dữ liệu liên quan
                    var shop = await _shopServiceClient.GetShopByIdAsync(shopOrder.ShopId);
                    if (shop == null) return Fail("Không tìm thấy cửa hàng");

                    var shopAddress = await _shopServiceClient.GetShopAddressAsync(shopOrder.ShopId);
                    if (shopAddress == null) return Fail("Không tìm thấy địa chỉ cửa hàng");

                    var customerAddress = await _addressServiceClient.GetCustomerAddress(request.AddressId, accessToken);
                    if (customerAddress == null) return Fail("Không tìm thấy địa chỉ người nhận");

                    var shopMembership = await _membershipServiceClient.GetShopMembershipDTO(shopOrder.ShopId.ToString());
                    if (shopMembership == null) return Fail("Không tìm thấy gói thành viên của cửa hàng");

                    // Tạo đơn hàng và item
                    var order = CreateOrder(request, shopOrder, shopAddress, customerAddress);
                  
                    order.SetCreator(request.AccountId.ToString());
                    var itemResult = await BuildOrderItemsAsync(order, shopOrder.Items, shopMembership);
                    if (!itemResult.Success) return Fail(itemResult.Message);

                    order.AddItems(itemResult.Data);
                    order.ShippingFee = shopOrder.ShippingFee;
                    if (shopOrder.ExpectedDeliveryDay != null)
                    {
                        order.EstimatedDeliveryDate = DateTime.SpecifyKind(shopOrder.ExpectedDeliveryDay, DateTimeKind.Utc);
                    }
                    // Tính giá trị đơn hàng
                    decimal voucherDiscount = 0;
                    decimal commissionRate = shopMembership.Commission ?? 0;

                    CalculateOrderTotals(order, commissionRate, voucherDiscount);

                    order.SetPaymentMethod(request.PaymentMethod, request.AccountId.ToString());
                    if (request.PaymentMethod == "COD")
                    {
                        order.OrderStatus = Domain.Enums.OrderStatus.Pending;
                        SetTimeForShopIfExists(order, DateTime.UtcNow.AddHours(24));
                    }
                    else
                    {
                       order.OrderStatus = Domain.Enums.OrderStatus.Waiting;

                    }
                    // Lưu đơn hàng ban đầu
                    await _orderRepository.InsertAsync(order);
                    foreach (var orderItem in itemResult.Data)
                    {
                        await UpdateStockBasedOnOrderTypeAsync(orderItem, request.LivestreamId);
                        if (orderItem.VariantId.HasValue)
                        {
                            
                            var productStockSuccess = await _productServiceClient.UpdateProductStockAsync(
                                orderItem.ProductId,
                                -orderItem.Quantity); 

                            var variantStockSuccess = await _productServiceClient.UpdateVariantStockAsync(
                                orderItem.VariantId.Value,
                                -orderItem.Quantity);
                            //var quantitySoldSuccess = await _productServiceClient.UpdateProductQuantitySoldAsync(
                            //    orderItem.ProductId,
                            //    orderItem.Quantity);
                            if (!productStockSuccess || !variantStockSuccess)
                            {
                                _logger.LogError("Failed to update stock for product {ProductId} variant {VariantId}",
                                    orderItem.ProductId, orderItem.VariantId.Value);
                            }
                            else
                            {
                                _logger.LogInformation("✅ Updated stock: Product {ProductId} and Variant {VariantId} decreased by {Quantity}",
                                    orderItem.ProductId, orderItem.VariantId.Value, orderItem.Quantity);
                                // Cập nhật FlashSale đã bán nếu có
                                if (!variantCache.TryGetValue(orderItem.VariantId.Value, out var variantDto))
                                {
                                    variantDto = await _productServiceClient.GetVariantByIdAsync(orderItem.VariantId.Value);
                                    if (variantDto != null) variantCache[orderItem.VariantId.Value] = variantDto;
                                }

                                if (variantDto?.FlashSalePrice.HasValue == true && variantDto.FlashSalePrice.Value > 0)
                                {
                                    if (flashIndex.TryGetValue((orderItem.ProductId, orderItem.VariantId.Value), out var flashSaleId))
                                    {
                                        var ok = await _productServiceClient.IncreaseFlashSaleSoldAsync(flashSaleId, orderItem.Quantity);
                                        if (!ok)
                                        {
                                            _logger.LogWarning("Failed to increase flash sale sold. FS={FlashSaleId}, Product={ProductId}, Variant={VariantId}",
                                                flashSaleId, orderItem.ProductId, orderItem.VariantId.Value);
                                        }
                                        else
                                        {
                                            _logger.LogInformation("✅ Increased FS sold: FS={FlashSaleId} +{Qty}", flashSaleId, orderItem.Quantity);
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogDebug("No active FlashSale mapped for Product {ProductId}, Variant {VariantId}",
                                            orderItem.ProductId, orderItem.VariantId.Value);
                                    }
                                }

                            }
                        }
                        else
                        {
                            var stockUpdateSuccess = await _productServiceClient.UpdateProductStockAsync(
                                orderItem.ProductId,
                                -orderItem.Quantity); 
                            //var quantitySoldSuccess = await _productServiceClient.UpdateProductQuantitySoldAsync(
                            //    orderItem.ProductId,
                            //    orderItem.Quantity);
                            if (!stockUpdateSuccess)
                            {
                                _logger.LogError("Failed to update stock for product {ProductId}", orderItem.ProductId);
                                // Có thể rollback order nếu cần
                            }
                            else
                            {
                                _logger.LogInformation("✅ Updated stock: Product {ProductId} decreased by {Quantity}",
                                    orderItem.ProductId, orderItem.Quantity);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(shopOrder.VoucherCode))
                    {
                        _logger.LogInformation("Applying voucher {Code} for shop {ShopId}, order amount: {Amount}đ",
                            shopOrder.VoucherCode, shopOrder.ShopId, order.FinalAmount);

                        var voucherResult = await ApplyVoucherAsync(order, shopOrder.VoucherCode, accessToken, shopOrder.ShopId);
                        if (!voucherResult.Success)
                        {
                            _logger.LogWarning("Lỗi khi dùng voucher", voucherResult.Message);
                            return Fail(voucherResult.Message);
                        }

                        _logger.LogInformation("✅ Voucher applied successfully. Discount: {Discount}đ",
                            voucherResult.Data.DiscountAmount);
                        var itemDiscountTotal = itemResult.Data.Sum(x => x.DiscountAmount);
                        var voucherDiscountAmount = voucherResult.Data.DiscountAmount;
                        order.VoucherCode = voucherResult.Data.VoucherCode;
                        order.DiscountAmount =  voucherDiscountAmount;
                        order.FinalAmount = voucherResult.Data.FinalAmount;

                        _logger.LogInformation("📊 Order updated - Item Discount: {ItemDiscount}đ, Voucher Discount: {VoucherDiscount}đ, Final: {Final}đ",
                            itemDiscountTotal, voucherDiscountAmount, order.FinalAmount);
                        await _orderRepository.ReplaceAsync(order.Id.ToString(), order);
                    }

                    // Gửi sự kiện
                    await PublishOrderEventsAsync(order, request.PaymentMethod, request.AccountId);

                    // Schedule auto-cancel / deadline chain
                    if (string.Equals(request.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase))
                    {
                      
                        SchedulePendingThenProcessingThenShippedDeadlines(order.Id);
                    }
                    else
                    {
                        ScheduleBankTransferDeadlines(order.Id);
                    }

                    // Tạo DTO trả về
                    var orderDto = await BuildOrderDtoAsync(order);
                    response.Data.Add(orderDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating order for shop {ShopId}", shopOrder.ShopId);
                    return Fail("Lỗi khi tạo đơn hàng");
                }
            }

            return response;

            // Helper fail response
            ApiResponse<List<OrderDto>> Fail(string msg) => new()
            {
                Success = false,
                Message = msg,
                Data = new List<OrderDto>()
            };
        }
        private async Task UpdateStockBasedOnOrderTypeAsync(OrderItem orderItem, Guid? livestreamId)
        {
            try
            {
                if (livestreamId.HasValue)
                {
                    // ✅ LIVESTREAM ORDER: CHỈ cập nhật stock của livestream product
                    _logger.LogInformation("🎥 Processing LIVESTREAM order - updating livestream product stock only for livestreamId {LivestreamId}, productId {ProductId}",
                        livestreamId.Value, orderItem.ProductId);

                    await UpdateLivestreamProductStockAsync(livestreamId.Value, orderItem);
                }
                else
                {
                    // ✅ NORMAL ORDER: Cập nhật stock của Product Service như bình thường
                    _logger.LogInformation("🛒 Processing NORMAL order - updating product service stock for productId {ProductId}",
                        orderItem.ProductId);

                    await UpdateProductServiceStockAsync(orderItem);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating stock for product {ProductId}", orderItem.ProductId);
            }
        }
        private async Task UpdateProductServiceStockAsync(OrderItem orderItem)
        {
            try
            {
                bool productStockSuccess = false;
                bool variantStockSuccess = true; // Default true for non-variant products

                if (orderItem.VariantId.HasValue)
                {
                    productStockSuccess = await _productServiceClient.UpdateProductStockAsync(
                        orderItem.ProductId,
                        -orderItem.Quantity);

                    variantStockSuccess = await _productServiceClient.UpdateVariantStockAsync(
                        orderItem.VariantId.Value,
                        -orderItem.Quantity);

                    if (!productStockSuccess || !variantStockSuccess)
                    {
                        _logger.LogError("❌ Failed to update Product Service stock for product {ProductId} variant {VariantId}",
                            orderItem.ProductId, orderItem.VariantId.Value);
                    }
                    else
                    {
                        _logger.LogInformation("✅ Updated Product Service stock: Product {ProductId} and Variant {VariantId} decreased by {Quantity}",
                            orderItem.ProductId, orderItem.VariantId.Value, orderItem.Quantity);
                    }
                }
                else
                {
                    productStockSuccess = await _productServiceClient.UpdateProductStockAsync(
                        orderItem.ProductId,
                        -orderItem.Quantity);

                    if (!productStockSuccess)
                    {
                        _logger.LogError("❌ Failed to update Product Service stock for product {ProductId}", orderItem.ProductId);
                    }
                    else
                    {
                        _logger.LogInformation("✅ Updated Product Service stock: Product {ProductId} decreased by {Quantity}",
                            orderItem.ProductId, orderItem.Quantity);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating Product Service stock for product {ProductId}", orderItem.ProductId);
            }
        }
        private async Task UpdateLivestreamProductStockAsync(Guid livestreamId, OrderItem orderItem)
        {
            try
            {
                // Gọi Livestream Service để cập nhật stock
                var livestreamStockUpdateSuccess = await _livestreamServiceClient.UpdateProductStockAsync(
                    livestreamId,
                    orderItem.ProductId.ToString(),
                    orderItem.VariantId?.ToString(),
                    -orderItem.Quantity, 
                    "order-creation");

                if (livestreamStockUpdateSuccess)
                {
                    _logger.LogInformation("✅ Updated Livestream Service stock: Product {ProductId} in livestream {LivestreamId} decreased by {Quantity}",
                        orderItem.ProductId, livestreamId, orderItem.Quantity);
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to update livestream product stock for product {ProductId} in livestream {LivestreamId}",
                        orderItem.ProductId, livestreamId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating livestream product stock for product {ProductId} in livestream {LivestreamId}",
                    orderItem.ProductId, livestreamId);
            }
        }
        private Orders CreateOrder(CreateMultiOrderCommand request, CreateOrderByShopDto shopOrder, AddressOfShop shopAddress, AdressDto customerAddress)
        {
            return new Orders(
                accountId: request.AccountId,
                shopId: shopOrder.ShopId,
                toName: customerAddress.RecipientName,
                toPhone: customerAddress.PhoneNumber,
                toAddress: string.Join(", ", new[] { customerAddress.Street, customerAddress.Ward, customerAddress.District, customerAddress.City, customerAddress.Country }.Where(x => !string.IsNullOrWhiteSpace(x))),
                toWard: customerAddress.Ward,
                toDistrict: customerAddress.District,
                toProvince: customerAddress.City,
                toPostalCode: customerAddress.PostalCode,
                fromAddress: shopAddress.Street,
                fromWard: shopAddress.Ward,
                fromDistrict: shopAddress.District,
                fromProvince: shopAddress.City,
                fromPostalCode: shopAddress.PostalCode,
                fromShop: shopAddress.RecipientName,
                fromPhone: shopAddress.PhoneNumber,
                shippingProviderId: shopOrder.ShippingProviderId,
                customerNotes: shopOrder.CustomerNotes,
                livestreamId: request.LivestreamId,
                createdFromCommentId: request.CreatedFromCommentId,
                paymentMethod: request.PaymentMethod 

            );
        }
        private async Task<ApiResponse<List<OrderItem>>> BuildOrderItemsAsync(Orders order, List<CreateOrderItemDto> items, DetailMembershipDTO membership)
        {
            var orderItems = new List<OrderItem>();

            foreach (var item in items)
            {
                var product = await _productServiceClient.GetProductByIdAsync(item.ProductId);
                if (product == null)
                    return Fail($"Không tìm thấy sản phẩm có mã: {item.ProductId}");

                decimal unitPrice = 0;
                decimal discount = 0;

                if (item.VariantId.HasValue)
                {
                    var variant = await _productServiceClient.GetVariantByIdAsync(item.VariantId.Value);
                    if (variant == null)
                        return Fail($"Không tìm thấy phiên bản sản phẩm có mã: {item.VariantId}");

                    unitPrice = (decimal)variant.FinalPrice;

                    // Kiểm tra null và tránh chia cho 0
                    if (variant.FlashSalePrice.HasValue && variant.FlashSalePrice.Value > 0)
                    {
                        unitPrice = variant.FlashSalePrice.Value;
                        discount = (decimal)(variant.Price - variant.FinalPrice);
                    }
                }
                else
                {
                    unitPrice = product.FinalPrice;

                    if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
                    {
                        unitPrice = product.FinalPrice;
                        discount = (decimal)(product.BasePrice - product.FinalPrice);
                    }
                }

                orderItems.Add(new OrderItem(
                    orderId: order.Id,
                    productId: item.ProductId,
                    quantity: item.Quantity,
                    unitPrice: unitPrice,
                    discountAmount: discount * item.Quantity,
                    variantId: item.VariantId
                ));
                
            }

            return Success(orderItems);

            // Helper functions
            ApiResponse<List<OrderItem>> Fail(string msg) => new() { Success = false, Message = msg };
            ApiResponse<List<OrderItem>> Success(List<OrderItem> data) => new() { Success = true, Data = data };
        }

        private async Task<ApiResponse<VoucherApplicationDto>> ApplyVoucherAsync(Orders order, string code, string accessToken, Guid shopId)
        {
            try
            {
                _logger.LogInformation("🎫 Validating voucher {Code} for shop {ShopId}, order amount: {Amount}",
                    code, shopId, order.FinalAmount);

                var validation = await _shopVoucherClientService.ValidateVoucherAsync(code, order.FinalAmount, shopId);
                if (validation == null || !validation.IsValid)
                {
                    _logger.LogWarning("❌ Voucher validation failed: {Message}", validation?.Message ?? "Unknown error");
                    return new ApiResponse<VoucherApplicationDto> { Success = false, Message = validation?.Message ?? "Không thể áp dụng voucher" };
                }

                _logger.LogInformation("✅ Voucher validation passed. Discount: {Discount}đ", validation.DiscountAmount);

                var applied = await _shopVoucherClientService.ApplyVoucherAsync(code, order.Id, order.FinalAmount, shopId, accessToken);
                if (applied != null && applied.IsApplied)
                {
                    _logger.LogInformation("🎉 Voucher applied successfully! Code: {Code}, Discount: {Discount}đ, Final: {Final}đ",
                        applied.VoucherCode, applied.DiscountAmount, applied.FinalAmount);
                    return new ApiResponse<VoucherApplicationDto> { Success = true, Data = applied, Message = "Voucher applied successfully" };
                }
                else
                {
                    _logger.LogWarning("❌ Voucher application failed: IsApplied={IsApplied}, Message={Message}",
                        applied?.IsApplied, applied?.Message ?? "Unknown error");
                    return new ApiResponse<VoucherApplicationDto> { Success = false, Message = applied?.Message ?? "Không thể áp dụng voucher" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error applying voucher {Code} for order {OrderId}", code, order.Id);
                return new ApiResponse<VoucherApplicationDto> { Success = false, Message = "Lỗi hệ thống khi áp dụng voucher" };
            }
        }
        private async Task PublishOrderEventsAsync(Orders order, string paymentMethod, Guid userId)
        {
            var orderItemEvent = order.Items.Select(x => new OrderItemInEvent
            {
                ProductId = x.ProductId.ToString(),
                VariantId = x.VariantId?.ToString(),
                Quantity = x.Quantity,
            }).ToList();

            if (paymentMethod == "COD")
            {
                var shopAccounts =await  _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                if (shopAccounts != null && shopAccounts.Any())
                {
                    var @event = new OrderCreatedOrUpdatedEvent
                    {
                        OrderCode = order.OrderCode,
                        Message = "đã được tạo thành công. Vui lòng chuẩn bị đơn hàng",
                        UserId = shopAccounts.Select(x => x.Id.ToString()).ToList(),
                        OrderStatus = "Pending",
                        OrderItems = orderItemEvent,
                        ShopId = order.ShopId.ToString(),
                        CreatedBy = order.CreatedBy,
                    };

                     _publishEndpoint.Publish(@event); // publish trực tiếp
                }
            }
            else
            {
                var @event = new OrderCreatedOrUpdatedEvent
                {
                    OrderCode = order.OrderCode,
                    Message = "đã được tạo thành công",
                    UserId = new List<string> { userId.ToString() },
                    OrderStatus = "Waiting",
                    OrderItems = orderItemEvent,
                    ShopId = order.ShopId.ToString(),
                    CreatedBy = order.CreatedBy,
                };

                 _publishEndpoint.Publish(@event); // publish trực tiếp
            }
        }
        private async Task<OrderDto> BuildOrderDtoAsync(Orders order)
        {
            var shippingDto = new ShippingAddressDto
            {
                FullName = order.ToName,
                Phone = order.ToPhone,
                AddressLine1 = order.ToAddress,
                Province = order.ToProvince,
                Ward = order.ToWard,
                District = order.ToDistrict,
                PostalCode = order.ToPostalCode,
                Country = "Vietnam"
            };

            var itemDtos = new List<OrderItemDto>();
            foreach (var item in order.Items)
            {
                var product = await _productServiceClient.GetProductByIdAsync(item.ProductId);
                itemDtos.Add(new OrderItemDto
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountAmount = item.DiscountAmount,
                    TotalPrice = item.TotalPrice,
                    Notes = item.Notes,
                    ProductName = product?.ProductName ?? "Unknown Product",
                    ProductImageUrl = product?.PrimaryImageUrl ?? string.Empty
                });
            }

            return new OrderDto
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                AccountId = order.AccountId,
                ShopId = order.ShopId,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                ShippingAddress = shippingDto,
                ShippingProviderId = order.ShippingProviderId,
                ShippingFee = order.ShippingFee,
                TotalPrice = order.TotalPrice,
                DiscountAmount = order.DiscountAmount,
                FinalAmount = order.FinalAmount,
                CustomerNotes = order.CustomerNotes,
                TrackingCode = order.TrackingCode,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                ActualDeliveryDate = order.ActualDeliveryDate,
                LivestreamId = order.LivestreamId,
                TimeForShop = order.TimeForShop, 
                NetAmount = order.NetAmount,
                Items = itemDtos
            };
        }
        private void CalculateOrderTotals(Orders order, decimal commissionRate, decimal voucherDiscount = 0)
        {
            decimal totalPrice = order.Items.Sum(i => i.UnitPrice * i.Quantity);
            decimal itemDiscount = order.Items.Sum(i => i.DiscountAmount );
            decimal shippingFee = order.ShippingFee ;
            decimal commissionFee = totalPrice * commissionRate / 100;

            order.TotalPrice = totalPrice;
            order.DiscountAmount = voucherDiscount;
            order.FinalAmount = totalPrice - order.DiscountAmount + shippingFee;
            order.CommissionFee = order.TotalPrice * (commissionFee/100);
            order.NetAmount = totalPrice - commissionFee;
        }
        private void ScheduleBankTransferDeadlines(Guid orderId)
        {
            // 30-minute deadline to reach Pending (1)
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(30));
                    var order = await _orderRepository.GetByIdAsync(orderId.ToString());
                    if (order == null) return;

                    if (order.OrderStatus == OrderStatus.Waiting)
                    {
                        await CancelOrderAsync(order, "system-timeout-30m");
                        return;
                    }

                    if (order.OrderStatus >= OrderStatus.Pending)
                    {
                        // Start 24h chain: Pending -> Processing, then Processing -> Shipped
                        SetTimeForShopIfExists(order, DateTime.UtcNow.AddHours(24));
                        await _orderRepository.ReplaceAsync(order.Id.ToString(), order);
                        SchedulePendingThenProcessingThenShippedDeadlines(order.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scheduling bank transfer deadlines for order {OrderId}", orderId);
                }
            });
        }

        private void SchedulePendingThenProcessingThenShippedDeadlines(Guid orderId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // First 24h: must reach Processing (2)
                    await Task.Delay(TimeSpan.FromHours(24));
                    var order = await _orderRepository.GetByIdAsync(orderId.ToString());
                    if (order == null) return;

                    if (order.OrderStatus < OrderStatus.Processing && order.OrderStatus != OrderStatus.Cancelled)
                    {
                        await CancelOrderAsync(order, "system-timeout-24h-pending");
                        return;
                    }

                    if (order.OrderStatus >= OrderStatus.Processing && order.OrderStatus != OrderStatus.Cancelled)
                    {
                        // Extend time for shop another 24h
                        SetTimeForShopIfExists(order, DateTime.UtcNow.AddHours(24));
                        await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                        // Second 24h: must reach Shipped (3)
                        await Task.Delay(TimeSpan.FromHours(24));

                        var order2 = await _orderRepository.GetByIdAsync(orderId.ToString());
                        if (order2 == null) return;

                        if (order2.OrderStatus < OrderStatus.Shipped && order2.OrderStatus != OrderStatus.Cancelled)
                        {
                            await CancelOrderAsync(order2, "system-timeout-24h-processing");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error scheduling 24h deadlines for order {OrderId}", orderId);
                }
            });
        }

        private async Task CancelOrderAsync(Orders order, string reason)
        {
            try
            {
                order.UpdateStatus(OrderStatus.Cancelled, reason);
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                // Notify via event
                await PublishOrderEventsAsync(order, order.PaymentMethod ?? "Unknown", order.AccountId);
                _logger.LogInformation("Auto-cancelled order {OrderId} due to deadline ({Reason})", order.Id, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-cancelling order {OrderId}", order.Id);
            }
        }

        private void SetTimeForShopIfExists(Orders order, DateTime deadlineUtc)
        {
            try
            {
                var prop = order.GetType().GetProperty("TimeForShop", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.CanWrite && (prop.PropertyType == typeof(DateTime?) || prop.PropertyType == typeof(DateTime)))
                {
                    // Convert to matching type
                    object value = prop.PropertyType == typeof(DateTime) ? deadlineUtc : (DateTime?)deadlineUtc;
                    prop.SetValue(order, value);
                }
            }
            catch
            {
                // ignore if property not present
            }
        }

    }
}