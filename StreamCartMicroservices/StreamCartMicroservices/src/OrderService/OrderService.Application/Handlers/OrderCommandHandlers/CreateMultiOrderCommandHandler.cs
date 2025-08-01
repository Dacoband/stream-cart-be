    using MediatR;
    using Microsoft.Extensions.Logging;
    using OrderService.Application.Commands.OrderCommands;
    using OrderService.Application.DTOs.OrderDTOs;
    using OrderService.Application.DTOs.OrderItemDTOs;
    using OrderService.Application.Interfaces.IRepositories;
    using OrderService.Application.Interfaces.IServices;
    using OrderService.Domain.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Shared.Messaging.Event.OrderEvents;
    using MassTransit;
    using OrderService.Application.Interfaces;
    using Shared.Common.Models;
    using Shared.Common.Services.User;
using OrderService.Application.DTOs;

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

            public CreateMultiOrderCommandHandler(
                IOrderRepository orderRepository,
                IProductServiceClient productServiceClient,
                IShopServiceClient shopServiceClient,
                ILogger<CreateMultiOrderCommandHandler> logger,
                IPublishEndpoint publishEndpoint, IAdressServiceClient adressServiceClient, IMembershipServiceClient membershipServiceClient, IShopVoucherClientService shopVoucherClientService, IAccountServiceClient accountServiceClient, ICurrentUserService currentUserService)
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
                    var itemResult = await BuildOrderItemsAsync(order, shopOrder.Items, shopMembership);
                    if (!itemResult.Success) return Fail(itemResult.Message);

                    order.AddItems(itemResult.Data);
                    order.ShippingFee = shopOrder.ShippingFee;
                    order.EstimatedDeliveryDate = shopOrder.ExpectedDeliveryDay;

                    // Tính giá trị đơn hàng
                    decimal voucherDiscount = 0;
                    decimal commissionRate = shopMembership.Commission ?? 0;

                     CalculateOrderTotals(order, commissionRate, voucherDiscount);

                    // Lưu đơn hàng ban đầu
                    await _orderRepository.InsertAsync(order);                    // Áp dụng voucher nếu có
                    if (!string.IsNullOrEmpty(shopOrder.VoucherCode))
                    {
                        var voucherResult = await ApplyVoucherAsync(order, shopOrder.VoucherCode, accessToken, shopOrder.ShopId);
                        if (!voucherResult.Success) return Fail(voucherResult.Message);

                        order = voucherResult.Data;
                        voucherDiscount = voucherResult.Data.DiscountAmount - itemResult.Data.Sum(x => x.DiscountAmount);

                        // Cập nhật lại sau khi áp dụng voucher
                        CalculateOrderTotals(order, commissionRate, voucherDiscount);
                        await _orderRepository.ReplaceAsync(order.Id.ToString(), order);
                    }

                    // Gửi sự kiện
                    await PublishOrderEventsAsync(order, request.PaymentMethod, request.AccountId);

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
                createdFromCommentId: request.CreatedFromCommentId
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

                    unitPrice = variant.Price;

                    // Kiểm tra null và tránh chia cho 0
                    if (variant.FlashSalePrice.HasValue && variant.FlashSalePrice.Value > 0)
                    {
                        discount = unitPrice * (variant.FlashSalePrice.Value / 100m);
                    }
                }
                else
                {
                    unitPrice = product.BasePrice;

                    if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
                    {
                        discount = unitPrice * (product.DiscountPrice.Value / 100m);
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

        private async Task<ApiResponse<Orders>> ApplyVoucherAsync(Orders order, string code, string accessToken, Guid shopId)
        {
            var validation = await _shopVoucherClientService.ValidateVoucherAsync(code, order.FinalAmount, shopId);
            if (validation == null || !validation.IsValid)
                return new ApiResponse<Orders> { Success = false, Message = "Không thể áp dụng voucher" };

            var applied = await _shopVoucherClientService.ApplyVoucherAsync(code, order.Id, order.FinalAmount, accessToken);
            if (applied != null && applied.IsApplied)
            {
                order.DiscountAmount += applied.DiscountAmount;
                order.FinalAmount = applied.FinalAmount;
                order.VoucherCode = applied.VoucherCode;
            }

            return new ApiResponse<Orders> { Success = true, Data = order };
        }
        private async Task PublishOrderEventsAsync(Orders order, string paymentMethod, Guid userId)
        {
            await _publishEndpoint.Publish(new OrderCreatedOrUpdatedEvent
            {
                OrderCode = order.OrderCode,
                Message = "đã được tạo thành công",
                UserId = userId.ToString()
            });

            if (paymentMethod == "COD")
            {
                var shopAccounts = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                if
                    (shopAccounts != null)
                {
                    foreach (var acc in shopAccounts)
                    {
                        await _publishEndpoint.Publish(new OrderCreatedOrUpdatedEvent
                        {
                            OrderCode = order.OrderCode,
                            Message = "đã được tạo thành công. Vui lòng chuẩn bị đơn hàng",
                            UserId = acc.Id.ToString()
                        });
                    }

                }
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
            order.DiscountAmount = itemDiscount + voucherDiscount;
            order.FinalAmount = totalPrice - order.DiscountAmount + shippingFee;
            order.CommissionFee = order.TotalPrice * (commissionFee/100);
            order.NetAmount = totalPrice - commissionFee;
        }


    }
}