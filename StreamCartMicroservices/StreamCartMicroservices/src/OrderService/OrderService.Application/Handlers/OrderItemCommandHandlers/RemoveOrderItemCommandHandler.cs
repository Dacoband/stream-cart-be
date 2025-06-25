using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderItemCommands;
using OrderService.Application.Interfaces.IRepositories;

namespace OrderService.Application.Handlers.OrderItemCommandHandlers
{
    public class RemoveOrderItemCommandHandler : IRequestHandler<RemoveOrderItemCommand, bool>
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<RemoveOrderItemCommandHandler> _logger;

        public RemoveOrderItemCommandHandler(
            IOrderItemRepository orderItemRepository,
            IOrderRepository orderRepository,
            ILogger<RemoveOrderItemCommandHandler> logger)
        {
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(RemoveOrderItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Removing order item with ID {OrderItemId}", request.Id);

                var orderItem = await _orderItemRepository.GetByIdAsync(request.Id.ToString());
                if (orderItem == null)
                {
                    _logger.LogWarning("Order item with ID {OrderItemId} not found", request.Id);
                    return false;
                }

                var orderId = orderItem.OrderId;
                var order = await _orderRepository.GetByIdAsync(orderId.ToString());
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                    return false;
                }

                order.RemoveItem(orderItem.Id);

                await _orderRepository.ReplaceAsync(order.Id.ToString(), order);

                await _orderItemRepository.DeleteAsync(request.Id.ToString());

                _logger.LogInformation("Order item with ID {OrderItemId} removed successfully", request.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order item: {ErrorMessage}", ex.Message);
                return false;
            }
        }
    }
}