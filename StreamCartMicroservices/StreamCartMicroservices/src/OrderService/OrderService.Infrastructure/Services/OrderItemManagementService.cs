using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderItemCommands;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderItemQueries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Services
{
    public class OrderItemManagementService : IOrderItemService
    {
        private readonly IMediator _mediator;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderRepository _orderRepository; 
        private readonly ILogger<OrderItemManagementService> _logger;

        public OrderItemManagementService(
            IMediator mediator,
            IOrderItemRepository orderItemRepository,
            IOrderRepository orderRepository, 
            ILogger<OrderItemManagementService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderItemDto> GetOrderItemByIdAsync(Guid orderItemId)
        {
            try
            {
                _logger.LogInformation("Getting order item {OrderItemId}", orderItemId);
                var query = new GetOrderItemByIdQuery { OrderItemId = orderItemId };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order item by ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<OrderItemDto>> GetOrderItemsByOrderIdAsync(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Getting items for order {OrderId}", orderId);
                var query = new GetOrderItemsByOrderIdQuery { OrderId = orderId };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order items by order ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderItemDto> CreateOrderItemAsync(CreateOrderItemDto createOrderItemDto)
        {
            try
            {
                _logger.LogInformation("Creating new order item for product {ProductId}", createOrderItemDto.ProductId);

                var command = new CreateOrderItemCommand
                {
                    OrderId = Guid.Empty,
                    ProductId = createOrderItemDto.ProductId,
                    VariantId = createOrderItemDto.VariantId,
                    Quantity = createOrderItemDto.Quantity,
                    UnitPrice = createOrderItemDto.UnitPrice,
                    //Notes = createOrderItemDto.Notes
                };
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order item: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderItemDto> UpdateOrderItemAsync(
            Guid orderItemId,
            int quantity,
            decimal unitPrice,
            decimal discountAmount,
            string notes,
            string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Updating order item {OrderItemId}", orderItemId);
                var command = new UpdateOrderItemCommand
                {
                    Id = orderItemId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    DiscountAmount = discountAmount,
                    Notes = notes,
                    ModifiedBy = modifiedBy
                };
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order item: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<bool> RemoveOrderItemAsync(Guid orderItemId, string removedBy)
        {
            try
            {
                _logger.LogInformation("Removing order item {OrderItemId}", orderItemId);
                var command = new RemoveOrderItemCommand
                {
                    Id = orderItemId,
                    RemovedBy = removedBy
                };
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order item: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<ProductSalesStatisticsDto> GetProductSalesStatisticsAsync(
            Guid productId,
            Guid? shopId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting sales statistics for product {ProductId}", productId);
                var query = new GetProductSalesStatisticsQuery
                {
                    ProductId = productId,
                    ShopId = shopId,
                    StartDate = startDate,
                    EndDate = endDate
                };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product sales statistics: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<ProductSalesStatisticsDto>> GetShopSalesStatisticsAsync(
            Guid shopId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? topProductsLimit = null)
        {
            try
            {
                _logger.LogInformation("Getting sales statistics for shop {ShopId}", shopId);
                var query = new GetShopSalesStatisticsQuery
                {
                    ShopId = shopId,
                    StartDate = startDate,
                    EndDate = endDate,
                    TopProductsLimit = topProductsLimit
                };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shop sales statistics: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderItemDto> ApplyRefundAsync(Guid orderItemId, Guid refundRequestId, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Applying refund to order item {OrderItemId}", orderItemId);

                // Đầu tiên, lấy thông tin orderItem hiện tại
                var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId.ToString());
                if (orderItem == null)
                {
                    throw new ApplicationException($"Order item with ID {orderItemId} not found");
                }

                var command = new UpdateOrderItemCommand
                {
                    Id = orderItemId,
                    Quantity = orderItem.Quantity,
                    UnitPrice = orderItem.UnitPrice,
                    DiscountAmount = orderItem.DiscountAmount,
                    Notes = orderItem.Notes,
                    ModifiedBy = modifiedBy,
                    // Thêm trường RefundRequestId nếu UpdateOrderItemCommand có hỗ trợ
                    // RefundRequestId = refundRequestId
                };

                var updatedItem = await _mediator.Send(command);

                
                orderItem.LinkToRefundRequest(refundRequestId, modifiedBy);
                await _orderItemRepository.ReplaceAsync(orderItem.Id.ToString(), orderItem);

                return await GetOrderItemByIdAsync(orderItemId); // Lấy lại item đã cập nhật
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying refund: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateOrderItemAsync(CreateOrderItemDto createOrderItemDto)
        {
            try
            {
                if (createOrderItemDto == null)
                {
                    return (false, "Order item data cannot be null");
                }

                // Bỏ kiểm tra OrderId vì nó không có trong CreateOrderItemDto
                // if (createOrderItemDto.OrderId == Guid.Empty)
                // {
                //     return (false, "Order ID is required");
                // }

                if (createOrderItemDto.ProductId == Guid.Empty)
                {
                    return (false, "Product ID is required");
                }

                if (createOrderItemDto.Quantity <= 0)
                {
                    return (false, "Quantity must be greater than zero");
                }

                if (createOrderItemDto.UnitPrice < 0)
                {
                    return (false, "Unit price cannot be negative");
                }

                // Bỏ kiểm tra Order tồn tại vì OrderId không có trong DTO
                // Nếu cần kiểm tra Order tồn tại, phải truyền orderId riêng:
                // var order = await _orderRepository.GetByIdAsync(orderId.ToString());
                // if (order == null)
                // {
                //     return (false, $"Order with ID {orderId} does not exist");
                // }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating order item: {ErrorMessage}", ex.Message);
                return (false, $"Validation error: {ex.Message}");
            }
        }
    }
}