using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderItemQueries;

namespace OrderService.Application.Handlers.OrderItemQueryHandlers
{
    public class GetOrderItemByIdQueryHandler : IRequestHandler<GetOrderItemByIdQuery, OrderItemDto>
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetOrderItemByIdQueryHandler> _logger;

        public GetOrderItemByIdQueryHandler(
            IOrderItemRepository orderItemRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetOrderItemByIdQueryHandler> logger)
        {
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderItemDto> Handle(GetOrderItemByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting order item with ID {OrderItemId}", request.OrderItemId);

                var orderItem = await _orderItemRepository.GetByIdAsync(request.OrderItemId.ToString());
                if (orderItem == null)
                {
                    _logger.LogWarning("Order item with ID {OrderItemId} not found", request.OrderItemId);
                    return null;
                }

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

                return orderItemDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order item by ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}