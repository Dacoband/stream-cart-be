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
            var result = new ApiResponse<List<OrderDto>>()
            {
                Success = true,
                Message = "Tạo đơn hàng thành công",
            };
            _logger.LogInformation("Creating multiple orders for account {AccountId}", request.AccountId);
            var createdOrders = new List<OrderDto>();
            var accesstoken = _currentUserService.GetAccessToken();
            foreach (var orderByShop in request.OrdersByShop)
            {
                try
                {
                    // Validate shop exists
                    var shop = await _shopServiceClient.GetShopByIdAsync(orderByShop.ShopId);
                    if (shop == null)
                    {
                        result.Success = false;
                        result.Message = "Không tìm thấy cửa hàng";
                        return result;
                    }

                    // Get shop address for shipping from
                    var shopAddress = await _shopServiceClient.GetShopAddressAsync(orderByShop.ShopId);
                    if (shopAddress == null)
                    {
                        result.Success = false;
                        result.Message = "Không tìm thấy địa chỉ cửa hàng";
                        return result;
                    }
                    //Get Customer Address
                    var customerAddress = await _addressServiceClient.GetCustomerAddress(request.AddressId);
                    if(customerAddress == null)
                    {
                        result.Success = false;
                        result.Message = "Không tìm thấy địa chỉ người nhận";
                        return result;
                    }
                    // Create the order
                    var order = new Orders(
                        accountId: request.AccountId,
                        shopId: orderByShop.ShopId,
                        toName: customerAddress.RecipientName,
                        toPhone: customerAddress.PhoneNumber,
                        toAddress: string.Join(", ",
                            new[] {
                                customerAddress.Street,
                                customerAddress.Ward,
                                customerAddress.District,
                                customerAddress.City,
                                customerAddress.Country
                            }.Where(x => !string.IsNullOrWhiteSpace(x))
                        ),
                        toWard:  customerAddress.Ward,
                        toDistrict: customerAddress.District,
                        toProvince: customerAddress.City,
                        toPostalCode: customerAddress.PostalCode,
                        fromAddress: shopAddress.Address,
                        fromWard: shopAddress.Ward,
                        fromDistrict: shopAddress.District,
                        fromProvince: shopAddress.City,
                        fromPostalCode: shopAddress.PostalCode,
                        fromShop: shopAddress.Name,
                        fromPhone: shopAddress.PhoneNumber,
                        shippingProviderId: orderByShop.ShippingProviderId,
                        customerNotes: orderByShop.CustomerNotes,
                        livestreamId: request.LivestreamId,
                        createdFromCommentId: request.CreatedFromCommentId
                        
                    );
                    order.ShippingFee = orderByShop.ShippingFee;
                    order.EstimatedDeliveryDate = orderByShop.ExpectedDeliveryDay;
                    //GetCommission
                    var shopMembership =await _membershipServiceClient.GetShopMembershipDTO(orderByShop.ShopId.ToString());
                    if (shopMembership == null)
                    {
                        result.Success = false;
                        result.Message = "Không tìm thấy gói thành viên của cửa hàng";
                        return result;
                    }
                    // Process order items
                    var orderItems = new List<OrderItem>();
                    decimal totalDiscount = 0;
                    decimal totalPrice = 0;
                    decimal finalAmount = 0;
                    decimal netAmount = 0;
                    decimal commision = (decimal)shopMembership.Commission/100;
                    foreach (var item in orderByShop.Items)
                    {
                        decimal productDiscount = 0;
                        var productDetails = await _productServiceClient.GetProductByIdAsync(item.ProductId);
                        if (productDetails == null)
                        {
                            result.Success = false;
                            result.Message = "Không tìm thấy sản phẩm";
                            return result;
                        }

                        decimal unitPrice = 0;
                        if (item.VariantId.HasValue)
                        {
                            var variantDetails = await _productServiceClient.GetVariantByIdAsync(item.VariantId.Value);
                            if (variantDetails == null)
                            {
                                _logger.LogWarning("Variant with ID {VariantId} not found", item.VariantId);
                                continue;
                            }
                            unitPrice = variantDetails.Price;
                            productDiscount = variantDetails.Price * ((decimal)variantDetails.FlashSalePrice / 100);

                        }
                        else
                        {
                            unitPrice = productDetails.BasePrice;
                            productDiscount = productDetails.BasePrice * ((decimal)productDetails.DiscountPrice / 100);
                        }

                        var orderItem = new OrderItem(
                            orderId: order.Id,
                            productId: item.ProductId,
                            quantity: item.Quantity,
                            unitPrice: unitPrice,
                            discountAmount : productDiscount * item.Quantity,
                            variantId : item.VariantId
                        );

                        orderItems.Add(orderItem);
                        totalPrice += orderItem.UnitPrice;
                        totalDiscount += orderItem.DiscountAmount;
                        finalAmount += orderItem.TotalPrice;
                        
                    }

                    // Add items to order
                    order.AddItems(orderItems);
                    netAmount = (decimal)(finalAmount - (finalAmount * shopMembership.Commission / 100));
                    // Save the order
                    try
                    {
                        await _orderRepository.InsertAsync(order);

                    }
                    catch (Exception ex) { 
                        result.Success = false;
                        result.Message = "Xảy ra lỗi trong quá trính tạo đơn hàng";
                        return result;
                    }
                    // Apply voucher discount if available
                    if (!string.IsNullOrEmpty(orderByShop.VoucherCode))
                    {
                        // Gọi hàm validate voucher
                        var validationResult = await _shopVoucherClientService.ValidateVoucherAsync(
                            orderByShop.VoucherCode,
                            finalAmount,
                            orderByShop.ShopId
                        );
                        if (validationResult == null || validationResult.IsValid == false ) {
                            result.Success = false;
                            result.Message = "Không thể áp dụng voucher cho đơn hàng";
                            return result;
                        }

                        if (validationResult != null && validationResult.IsValid)
                        {
                            // Nếu hợp lệ, gọi tiếp hàm apply voucher
                            var applicationResult = await _shopVoucherClientService.ApplyVoucherAsync(
                                orderByShop.VoucherCode,
                                order.Id,
                                finalAmount,
                                accesstoken
                            );

                            if (applicationResult != null && applicationResult.IsApplied)
                            {
                                order.DiscountAmount += applicationResult.DiscountAmount;
                                order.FinalAmount = applicationResult.FinalAmount;
                            }
                        }
                        try
                        {
                            await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                        }
                        catch(Exception ex)
                        {
                            result.Success = false;
                            result.Message = "Xảy ra lỗi trong quá trình áp dụng mã giảm giá";
                            return result ;
                        }
                        // Publish order created event
                        await _publishEndpoint.Publish(new OrderCreatedOrUpdatedEvent
                        {
                            OrderCode = order.OrderCode,
                            Message = "đã được tạo thành công",
                            UserId = request.AccountId.ToString()
                        });

                        _logger.LogInformation("Created order {OrderId} for shop {ShopId}", order.Id, order.ShopId);
                        if(request.PaymentMethod == "COD") {
                            var shopAccount = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                            foreach (var acc in shopAccount) {
                                await _publishEndpoint.Publish(new OrderCreatedOrUpdatedEvent
                                {
                                    OrderCode = order.OrderCode,
                                    Message = "đã được tạo thành công. Vui lòng chuẩn bị đơn hàng",
                                    UserId = acc.Id.ToString()
                                });

                            }


                        }
                    }

                   

                    // Create DTO for response
                    var shippingAddressDto = new ShippingAddressDto
                    {
                        FullName = order.ToName,
                        Phone = order.ToPhone,
                        AddressLine1 = order.ToAddress,
                        Ward = order.ToWard,
                        City = order.ToProvince,
                        State = order.ToDistrict,
                        PostalCode = order.ToPostalCode,
                        Country = "Vietnam",
                        IsDefault = false
                    };

                    var orderItemDtos = new List<OrderItemDto>();
                    foreach (var item in order.Items)
                    {
                        var productDetails = await _productServiceClient.GetProductByIdAsync(item.ProductId);

                        orderItemDtos.Add(new OrderItemDto
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
                            ProductName = productDetails?.ProductName ?? "Unknown Product",
                            ProductImageUrl = productDetails?.ImageUrl ?? string.Empty
                        });
                    }

                    var orderDto = new OrderDto
                    {
                        Id = order.Id,
                        OrderCode = order.OrderCode,
                        AccountId = order.AccountId,
                        ShopId = order.ShopId,
                        OrderDate = order.OrderDate,
                        OrderStatus = order.OrderStatus,
                        PaymentStatus = order.PaymentStatus,
                        ShippingAddress = shippingAddressDto,
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
                        Items = orderItemDtos
                    };

                    createdOrders.Add(orderDto);

                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating order for shop {ShopId}", orderByShop.ShopId);
                }
            }

            result.Data = createdOrders;
            return result;
        }
    }
}