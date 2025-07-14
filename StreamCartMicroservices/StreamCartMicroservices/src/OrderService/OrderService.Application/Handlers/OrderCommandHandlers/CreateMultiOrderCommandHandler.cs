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

namespace OrderService.Application.Handlers.OrderCommandHandlers
{
    public class CreateMultiOrderCommandHandler : IRequestHandler<CreateMultiOrderCommand, List<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<CreateMultiOrderCommandHandler> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateMultiOrderCommandHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            IShopServiceClient shopServiceClient,
            ILogger<CreateMultiOrderCommandHandler> logger,
            IPublishEndpoint publishEndpoint)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _shopServiceClient = shopServiceClient ?? throw new ArgumentNullException(nameof(shopServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishEndpoint = publishEndpoint;
        }

        public async Task<List<OrderDto>> Handle(CreateMultiOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating multiple orders for account {AccountId}", request.AccountId);
            var createdOrders = new List<OrderDto>();

            foreach (var orderByShop in request.OrdersByShop)
            {
                try
                {
                    // Validate shop exists
                    var shop = await _shopServiceClient.GetShopByIdAsync(orderByShop.ShopId);
                    if (shop == null)
                    {
                        _logger.LogWarning("Shop with ID {ShopId} not found", orderByShop.ShopId);
                        continue;
                    }

                    // Get shop address for shipping from
                    var shopAddress = await _shopServiceClient.GetShopAddressAsync(orderByShop.ShopId);
                    if (shopAddress == null)
                    {
                        _logger.LogWarning("Shop address not found for shop {ShopId}", orderByShop.ShopId);
                        continue;
                    }

                    // Create the order
                    var order = new Orders(
                        accountId: request.AccountId,
                        shopId: orderByShop.ShopId,
                        toName: request.ShippingAddress.FullName,
                        toPhone: request.ShippingAddress.Phone,
                        toAddress: request.ShippingAddress.AddressLine1,
                        toWard: request.ShippingAddress.AddressLine2 ?? request.ShippingAddress.Ward,
                        toDistrict: request.ShippingAddress.District,
                        toProvince: request.ShippingAddress.City ?? request.ShippingAddress.Province,
                        toPostalCode: request.ShippingAddress.PostalCode,
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

                    // Process order items
                    var orderItems = new List<OrderItem>();
                    decimal totalDiscount = 0;

                    foreach (var item in orderByShop.Items)
                    {
                        var productDetails = await _productServiceClient.GetProductByIdAsync(item.ProductId);
                        if (productDetails == null)
                        {
                            _logger.LogWarning("Product with ID {ProductId} not found", item.ProductId);
                            continue;
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
                        }
                        else
                        {
                            unitPrice = productDetails.BasePrice;
                        }

                        var orderItem = new OrderItem(
                            orderId: order.Id,
                            productId: item.ProductId,
                            quantity: item.Quantity,
                            unitPrice: unitPrice,
                            notes: string.Empty,
                            variantId: item.VariantId
                        );

                        orderItems.Add(orderItem);
                    }

                    // Add items to order
                    order.AddItems(orderItems);

                    // Apply voucher discount if available
                    if (orderByShop.VoucherId.HasValue)
                    {
                        // Here you would call a voucher service to get the discount amount
                        // For now, we'll just use a placeholder
                        // totalDiscount = await GetVoucherDiscountAmount(orderByShop.VoucherId.Value, order.TotalPrice);
                        // order.ApplyDiscount(totalDiscount, request.AccountId.ToString());
                    }

                    // Save the order
                    await _orderRepository.InsertAsync(order);

                    // Create DTO for response
                    var shippingAddressDto = new ShippingAddressDto
                    {
                        FullName = order.ToName,
                        Phone = order.ToPhone,
                        AddressLine1 = order.ToAddress,
                        AddressLine2 = order.ToWard,
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

                    // Publish order created event
                    await _publishEndpoint.Publish(new OrderCreatedOrUpdatedEvent
                    {
                        OrderCode = order.OrderCode,
                        Message = "đã được tạo thành công",
                        UserId = request.AccountId.ToString()
                    });

                    _logger.LogInformation("Created order {OrderId} for shop {ShopId}", order.Id, order.ShopId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating order for shop {ShopId}", orderByShop.ShopId);
                }
            }

            return createdOrders;
        }
    }
}