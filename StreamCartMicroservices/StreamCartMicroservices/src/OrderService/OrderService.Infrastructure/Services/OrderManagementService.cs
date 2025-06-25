using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderQueries;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Services
{
    public class OrderManagementService : IOrderService
    {
        private readonly IMediator _mediator;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderManagementService> _logger;

        public OrderManagementService(
            IMediator mediator,
            IOrderRepository orderRepository,
            ILogger<OrderManagementService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            try
            {
                _logger.LogInformation("Creating order for account {AccountId}", createOrderDto.AccountId);

                var command = new CreateOrderCommand
                {
                    AccountId = createOrderDto.AccountId,
                    ShopId = createOrderDto.ShopId,
                    CustomerName = string.Empty, // Not in DTO
                    CustomerEmail = string.Empty, // Not in DTO
                    CustomerPhone = string.Empty, // Not in DTO
                    ShippingAddress = createOrderDto.ShippingAddress,
                    PaymentMethod = string.Empty, // Not in DTO
                    ShippingMethod = string.Empty, // Not in DTO
                    ShippingFee = createOrderDto.ShippingAddress != null ? 0 : 0, // Default to zero if not specified
                    PromoCode = string.Empty, // Not in DTO
                    DiscountAmount = 0, // Default to zero if not specified
                    Notes = createOrderDto.CustomerNotes,
                    OrderItems = createOrderDto.Items?.Select(i => new CreateOrderItemDto
                    {
                        ProductId = i.ProductId,
                        VariantId = i.VariantId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        Notes = i.Notes
                    }).ToList() ?? new List<CreateOrderItemDto>()
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                _logger.LogInformation("Getting order {OrderId}", orderId);
                var query = new GetOrderByIdQuery { OrderId = orderId };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> GetOrderByCodeAsync(string orderCode)
        {
            try
            {
                _logger.LogInformation("Getting order by code {OrderCode}", orderCode);
                var query = new GetOrderByCodeQuery { OrderCode = orderCode };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by code: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<PagedResult<OrderDto>> GetOrdersByAccountIdAsync(Guid accountId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting orders for account {AccountId}", accountId);

                var searchParams = new OrderSearchParamsDto
                {
                    AccountId = accountId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var query = new SearchOrdersQuery { SearchParams = searchParams };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by account ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<PagedResult<OrderDto>> GetOrdersByShopIdAsync(Guid shopId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting orders for shop {ShopId}", shopId);
                var query = new GetOrdersByShopIdQuery
                {
                    ShopId = shopId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by shop ID: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<PagedResult<OrderDto>> SearchOrdersAsync(OrderSearchParamsDto searchParams)
        {
            try
            {
                _logger.LogInformation("Searching orders with parameters");
                var query = new SearchOrdersQuery { SearchParams = searchParams };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Updating order {OrderId} status to {Status}", orderId, newStatus);

                var command = new UpdateOrderStatusCommand
                {
                    OrderId = orderId,
                    NewStatus = newStatus,  
                    ModifiedBy = modifiedBy    
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> UpdatePaymentStatusAsync(Guid orderId, PaymentStatus newStatus, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Updating order {OrderId} payment status to {Status}", orderId, newStatus);

                var command = new UpdatePaymentStatusCommand
                {
                    OrderId = orderId,
                    NewStatus = newStatus,  
                    ModifiedBy = modifiedBy       
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> UpdateTrackingCodeAsync(Guid orderId, string trackingCode, string modifiedBy)
        {
            try
            {
                _logger.LogInformation("Updating tracking code for order {OrderId}", orderId);

                // Sửa command để sử dụng đúng thuộc tính
                var command = new UpdateTrackingCodeCommand
                {
                    OrderId = orderId,
                    TrackingCode = trackingCode,
                    ModifiedBy = modifiedBy      // Thay UpdatedBy thành ModifiedBy
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tracking code: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> UpdateShippingInfoAsync(
            Guid orderId,
            ShippingAddressDto shippingAddress,
            string shippingMethod = null,
            decimal? shippingFee = null,
            string modifiedBy = null)
        {
            try
            {
                _logger.LogInformation("Updating shipping info for order {OrderId}", orderId);

                var command = new UpdateShippingInfoCommand
                {
                    OrderId = orderId,
                    ShippingAddress = shippingAddress,
                    ShippingMethod = shippingMethod,
                    ShippingFee = shippingFee.HasValue ? (decimal)shippingFee : 0m, 
                    ModifiedBy = modifiedBy ?? string.Empty   
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping info: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderDto> CancelOrderAsync(Guid orderId, string cancelReason, string cancelledBy)
        {
            try
            {
                _logger.LogInformation("Cancelling order {OrderId}", orderId);
                var command = new CancelOrderCommand
                {
                    OrderId = orderId,
                    CancelReason = cancelReason,
                    CancelledBy = cancelledBy
                };
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(Guid shopId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Getting order statistics for shop {ShopId}", shopId);
                var query = new GetOrderStatisticsQuery
                {
                    ShopId = shopId,
                    StartDate = startDate,
                    EndDate = endDate
                };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateOrderAsync(CreateOrderDto createOrderDto)
        {
            try
            {
                if (createOrderDto == null)
                {
                    return (false, "Order data cannot be null");
                }

                if (createOrderDto.AccountId == Guid.Empty)
                {
                    return (false, "Account ID is required");
                }

                if (createOrderDto.ShippingAddress == null)
                {
                    return (false, "Shipping address is required");
                }

                if (string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress.FullName) ||
                    string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress.Phone) ||
                    string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress.AddressLine1) ||
                    string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress.City))
                {
                    return (false, "Shipping address details are incomplete");
                }

                if (createOrderDto.Items == null || !createOrderDto.Items.Any())
                {
                    return (false, "Order must contain at least one item");
                }

                foreach (var item in createOrderDto.Items)
                {
                    if (item.ProductId == Guid.Empty)
                    {
                        return (false, "Product ID is required for all items");
                    }

                    if (item.Quantity <= 0)
                    {
                        return (false, "Quantity must be greater than zero for all items");
                    }

                    if (item.UnitPrice <= 0)
                    {
                        return (false, "Unit price must be greater than zero for all items");
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating order: {ErrorMessage}", ex.Message);
                return (false, $"Validation error: {ex.Message}");
            }
        }
    }
}