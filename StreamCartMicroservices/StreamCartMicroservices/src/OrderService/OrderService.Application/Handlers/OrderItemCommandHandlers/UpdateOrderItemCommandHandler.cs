using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderItemCommands;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;

namespace OrderService.Application.Handlers.OrderItemCommandHandlers
{
    public class UpdateOrderItemCommandHandler : IRequestHandler<UpdateOrderItemCommand, OrderItemDto>
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<UpdateOrderItemCommandHandler> _logger;

        public UpdateOrderItemCommandHandler(
            IOrderItemRepository orderItemRepository,
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            ILogger<UpdateOrderItemCommandHandler> logger)
        {
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderItemDto> Handle(UpdateOrderItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating order item with ID {OrderItemId}", request.Id);

                var orderItem = await _orderItemRepository.GetByIdAsync(request.Id.ToString());
                if (orderItem == null)
                {
                    _logger.LogWarning("Order item with ID {OrderItemId} not found", request.Id);
                    throw new ApplicationException($"Order item with ID {request.Id} not found");
                }

                var order = await _orderRepository.GetByIdAsync(orderItem.OrderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", orderItem.OrderId);
                    throw new ApplicationException($"Order with ID {orderItem.OrderId} not found");
                }

                decimal oldTotalPrice = orderItem.TotalPrice;

                orderItem.UpdateQuantity(request.Quantity, request.ModifiedBy);
                orderItem.UpdateUnitPrice(request.UnitPrice, request.ModifiedBy);
                orderItem.ApplyDiscount(request.DiscountAmount, request.ModifiedBy);
                orderItem.UpdateNotes(request.Notes, request.ModifiedBy);

                await _orderItemRepository.ReplaceAsync(orderItem.Id.ToString(), orderItem);

                order.RemoveItem(orderItem.Id);
                order.AddItem(orderItem);

                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                var productDetails = await _productServiceClient.GetProductByIdAsync(orderItem.ProductId);

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
                    ProductName = productDetails?.ProductName ?? "Unknown Product",
                    ProductImageUrl = productDetails?.ImageUrl ?? string.Empty
                };

                _logger.LogInformation("Order item with ID {OrderItemId} updated successfully", request.Id);
                return orderItemDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order item: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}