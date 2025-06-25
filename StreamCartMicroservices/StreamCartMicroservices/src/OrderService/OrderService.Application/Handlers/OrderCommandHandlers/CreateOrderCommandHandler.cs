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

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductServiceClient productServiceClient,
            ILogger<CreateOrderCommandHandler> logger,IAccountServiceClient accountServiceClient)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountServiceClient = accountServiceClient ?? throw new ArgumentNullException(nameof(accountServiceClient));
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
                string addressLine2 = request.ShippingAddress.AddressLine2 ?? string.Empty;
                string city = request.ShippingAddress.City;
                string state = request.ShippingAddress.State ?? string.Empty;
                string postalCode = request.ShippingAddress.PostalCode ?? string.Empty;
                string country = request.ShippingAddress.Country;

                var order = new Orders(
                    request.AccountId,
                    request.ShopId ?? Guid.Empty,
                    fullName,                  // toName
                    phone,                     // toPhone
                    addressLine1,              // toAddress
                    string.Empty,              // toWard (not in ShippingAddressDto)
                    state,                     // toDistrict
                    city,                      // toProvince
                    postalCode,                // toPostalCode
                    // Shipping From information (sender)
                    "Shop Address",            // fromAddress (placeholder)
                    "Shop Ward",               // fromWard (placeholder)
                    "Shop District",           // fromDistrict (placeholder)
                    "Shop Province",           // fromProvince (placeholder)
                    "Shop PostalCode",         // fromPostalCode (placeholder)
                    "Shop Name",               // fromShop (placeholder)
                    "Shop Phone",              // fromPhone (placeholder)
                   request.ShippingProviderId ?? Guid.Empty,            // shippingProviderId (placeholder)
                    request.Notes               // customerNotes
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
                        itemDto.Notes,
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
                        AddressLine2 = addressLine2,
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