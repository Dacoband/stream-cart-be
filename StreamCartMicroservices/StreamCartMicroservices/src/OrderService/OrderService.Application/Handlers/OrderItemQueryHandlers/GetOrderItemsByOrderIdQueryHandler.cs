using System;
using System.Collections.Generic;
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
    public class GetOrderItemsByOrderIdQueryHandler : IRequestHandler<GetOrderItemsByOrderIdQuery, IEnumerable<OrderItemDto>>
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetOrderItemsByOrderIdQueryHandler> _logger;

        public GetOrderItemsByOrderIdQueryHandler(
            IOrderItemRepository orderItemRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetOrderItemsByOrderIdQueryHandler> logger)
        {
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<OrderItemDto>> Handle(GetOrderItemsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting order items for order {OrderId}", request.OrderId);

                var orderItems = await _orderItemRepository.GetByOrderIdAsync(request.OrderId);
                if (orderItems == null)
                {
                    _logger.LogWarning("No order items found for order {OrderId}", request.OrderId);
                    return new List<OrderItemDto>();
                }

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
                        RefundRequestId = item.RefundRequestId,
                        ProductName = productDetails?.ProductName ?? "Unknown Product",
                        ProductImageUrl = productDetails?.PrimaryImageUrl ?? string.Empty
                    });
                }

                return orderItemDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order items by order ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}