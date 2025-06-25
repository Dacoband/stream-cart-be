using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderItemCommands;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Entities;

namespace OrderService.Application.Handlers.OrderItemCommandHandlers
{
    public class CreateOrderItemCommandHandler : IRequestHandler<CreateOrderItemCommand, OrderItemDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<CreateOrderItemCommandHandler> _logger;

        public CreateOrderItemCommandHandler(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IProductServiceClient productServiceClient,
            ILogger<CreateOrderItemCommandHandler> logger)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderItemDto> Handle(CreateOrderItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating a new order item for order {OrderId}", request.OrderId);

                var order = await _orderRepository.GetByIdAsync(request.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Cannot create order item: Order with ID {OrderId} not found", request.OrderId);
                    throw new ApplicationException($"Order with ID {request.OrderId} not found");
                }

                var productDetails = await _productServiceClient.GetProductByIdAsync(request.ProductId);
                if (productDetails == null)
                {
                    _logger.LogWarning("Cannot create order item: Product with ID {ProductId} not found", request.ProductId);
                    throw new ApplicationException($"Product with ID {request.ProductId} not found");
                }

                var orderItem = new OrderItem(
                    request.OrderId,
                    request.ProductId,
                    request.Quantity,
                    request.UnitPrice,
                    request.Notes,
                    request.VariantId
                );

                await _orderItemRepository.InsertAsync(orderItem);

                order.AddItem(orderItem);

                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                var orderItemDto = new OrderItemDto
                {
                    Id = orderItem.Id,
                    OrderId = orderItem.OrderId,
                    ProductId = orderItem.ProductId,
                    VariantId = orderItem.VariantId,
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    DiscountAmount = orderItem.DiscountAmount,
                    TotalPrice = orderItem.TotalPrice,
                    Notes = orderItem.Notes,
                    RefundRequestId = orderItem.RefundRequestId,
                    ProductName = productDetails.ProductName,
                    ProductImageUrl = productDetails.ImageUrl ?? string.Empty
                };

                _logger.LogInformation("Order item created successfully for order {OrderId}", request.OrderId);
                return orderItemDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order item: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}