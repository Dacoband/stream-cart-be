using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Handlers.OrderCommandHandlers
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductServiceClient productServiceClient,
            ILogger<CreateOrderCommandHandler> logger,IAccountServiceClient accountServiceClient, IShopServiceClient shopServiceClient, IPublishEndpoint publishEndpoint)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountServiceClient = accountServiceClient ?? throw new ArgumentNullException(nameof(accountServiceClient));
            _shopServiceClient = shopServiceClient;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating a new order for account {AccountId}", request.AccountId);

                string orderCode = GenerateOrderCode();

                decimal totalPrice = 0;
                foreach (var item in request.OrderItems)
                {
                    totalPrice += item.UnitPrice * item.Quantity;
                }

                decimal finalAmount = totalPrice - request.DiscountAmount + request.ShippingFee;

                string fullName = request.ShippingAddress.FullName;
                string phone = request.ShippingAddress.Phone;
                string addressLine1 = request.ShippingAddress.AddressLine1;
                string addressLine2 = request.ShippingAddress.Ward ?? string.Empty;
                string city = request.ShippingAddress.City;
                string state = request.ShippingAddress.State ?? string.Empty;
                string postalCode = request.ShippingAddress.PostalCode ?? string.Empty;
                string country = request.ShippingAddress.Country;
                // Get shop details for sender information
                var shopDetails = await _shopServiceClient.GetShopAddressAsync(request.ShopId ?? Guid.Empty);
                if (shopDetails == null)
                {
                    throw new ApplicationException($"Shop with ID {request.ShopId} not found");
                }
                var order = new Orders(
                    request.AccountId,
                    request.ShopId ?? Guid.Empty,
                    request.ShippingAddress.FullName,      // toName
                    request.ShippingAddress.Phone,         // toPhone
                    request.ShippingAddress.AddressLine1,  // toAddress
                    request.ShippingAddress.Ward,          // toWard
                    request.ShippingAddress.District,      // toDistrict
                    request.ShippingAddress.City,          // toProvince (City actually maps to Province)
                    request.ShippingAddress.PostalCode,    // toPostalCode

                    // Shop information - this should come from the shop service
                    shopDetails?.Address ?? "Shop Address",       // fromAddress
                    shopDetails?.Ward ?? "Shop Ward",             // fromWard
                    shopDetails?.District ?? "Shop District",     // fromDistrict
                    shopDetails?.City ?? "Shop Province",         // fromProvince
                    shopDetails?.PostalCode ?? "Shop PostalCode", // fromPostalCode
                    shopDetails?.Name ?? "Shop Name",             // fromShop
                    shopDetails?.PhoneNumber ?? "Shop Phone",     // fromPhone

                    request.ShippingProviderId ?? Guid.Empty,     // shippingProviderId
                    request.Notes                                 // customerNotes
);

                // Apply shipping fee and discount to calculate final amount
                order.SetShippingFee(request.ShippingFee, request.AccountId.ToString());
                order.ApplyDiscount(request.DiscountAmount, request.AccountId.ToString());

                // Save the order
                await _orderRepository.InsertAsync(order);

                // Create and save order items
                var orderItems = new List<OrderItem>();
                foreach (var itemDto in request.OrderItems)
                {
                    var productDetails = await _productServiceClient.GetProductByIdAsync(itemDto.ProductId);

                    // Create order item using constructor
                    var orderItem = new OrderItem(
                        order.Id,
                        itemDto.ProductId,
                        itemDto.Quantity,
                        itemDto.UnitPrice,
                        "", // Notes can be set later if needed
                        itemDto.VariantId
                    );

                    await _orderItemRepository.InsertAsync(orderItem);
                    orderItems.Add(orderItem);

                    order.AddItem(orderItem);
                }
                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                var orderItemDtos = new List<OrderItemDto>();
                foreach (var item in orderItems)
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
                    ShippingAddress = new ShippingAddressDto
                    {
                        FullName = fullName,
                        Phone = phone,
                        AddressLine1 = addressLine1,
                        Ward = addressLine2,
                        City = city,
                        State = state,
                        PostalCode = postalCode,
                        Country = country,
                        IsDefault = false 
                    },
                    ShippingProviderId = order.ShippingProviderId,
                    ShippingFee = order.ShippingFee,
                    TotalPrice = order.TotalPrice,
                    DiscountAmount = order.DiscountAmount,
                    FinalAmount = order.FinalAmount,
                    CustomerNotes = order.CustomerNotes,
                    TrackingCode = order.TrackingCode,
                    LivestreamId = order.LivestreamId,
                    Items = orderItemDtos
                };

                _logger.LogInformation("Order created successfully with ID {OrderId} and Code {OrderCode}", order.Id, order.OrderCode);
                //pubish OrderChangeEvent to NotificationSevice
                var orderChangEvent = new OrderCreatedOrUpdatedEvent()
                {
                    OrderCode = order.OrderCode,
                    Message ="được tạo mới thành công",
                    UserId = request.AccountId.ToString(),
                };
                await _publishEndpoint.Publish(orderChangEvent);
                var shopAccount = await _accountServiceClient.GetAccountByShopIdAsync(order.ShopId);
                foreach (var acc in shopAccount)
                {
                    var ordChangeEvent = new OrderCreatedOrUpdatedEvent()
                    {
                        OrderCode = order.OrderCode,
                        Message = "vừa được cập nhật mã vận đơn",
                        UserId = acc.Id.ToString(),
                    };
                    await _publishEndpoint.Publish(ordChangeEvent);
                }
                return orderDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private string GenerateOrderCode()
        {
            return $"{DateTime.UtcNow:yyMMdd}{new Random().Next(1000, 9999)}";
        }
    }
}