using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderItemDTOs;
using OrderService.Application.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.Api.Controllers
{
    [Route("api/order-items")]
    [ApiController]
    public class OrderItemController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;
        private readonly ILogger<OrderItemController> _logger;

        public OrderItemController(IOrderItemService orderItemService, ILogger<OrderItemController> logger)
        {
            _orderItemService = orderItemService ?? throw new ArgumentNullException(nameof(orderItemService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets an order item by ID
        /// </summary>
        /// <param name="id">Order item ID</param>
        /// <returns>Order item details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderItemDto>> GetOrderItemById(Guid id)
        {
            try
            {
                var orderItem = await _orderItemService.GetOrderItemByIdAsync(id);
                if (orderItem == null)
                {
                    return NotFound();
                }
                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order item with ID {OrderItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order item");
            }
        }

        /// <summary>
        /// Gets all order items for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Collection of order items</returns>
        [HttpGet("by-order/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetOrderItemsByOrderId(Guid orderId)
        {
            try
            {
                var orderItems = await _orderItemService.GetOrderItemsByOrderIdAsync(orderId);
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for order {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order items");
            }
        }

        /// <summary>
        /// Creates a new order item
        /// </summary>
        /// <param name="orderId">Order ID to add the item to</param>
        /// <param name="createOrderItemDto">Order item data</param>
        /// <returns>Created order item</returns>
        [HttpPost("orders/{orderId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderItemDto>> CreateOrderItem(Guid orderId, [FromBody] CreateOrderItemDto createOrderItemDto)
        {
            try
            {
                // Validate the order item
                var (isValid, errorMessage) = await _orderItemService.ValidateOrderItemAsync(createOrderItemDto);
                if (!isValid)
                {
                    return BadRequest(errorMessage);
                }

                // In your actual service, you'll need to set the OrderId for the item
                // The current implementation stores OrderId separately from the DTO
                var orderItem = await _orderItemService.CreateOrderItemAsync(createOrderItemDto);

                return CreatedAtAction(nameof(GetOrderItemById), new { id = orderItem.Id }, orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order item for order {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the order item");
            }
        }

        /// <summary>
        /// Updates an existing order item
        /// </summary>
        /// <param name="id">Order item ID</param>
        /// <param name="updateDto">Update data</param>
        /// <returns>Updated order item</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderItemDto>> UpdateOrderItem(Guid id, [FromBody] UpdateOrderItemDto updateDto)
        {
            try
            {
                var modifiedBy = User.Identity?.IsAuthenticated == true ?
                    User.Identity.Name : "system";

                var orderItem = await _orderItemService.UpdateOrderItemAsync(
                    id,
                    updateDto.Quantity,
                    updateDto.UnitPrice,
                    updateDto.DiscountAmount,
                    updateDto.Notes,
                    modifiedBy);

                if (orderItem == null)
                {
                    return NotFound();
                }

                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order item {OrderItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order item");
            }
        }

        /// <summary>
        /// Removes an order item
        /// </summary>
        /// <param name="id">Order item ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RemoveOrderItem(Guid id)
        {
            try
            {
                var removedBy = User.Identity?.IsAuthenticated == true ?
                    User.Identity.Name : "system";

                var result = await _orderItemService.RemoveOrderItemAsync(id, removedBy);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order item {OrderItemId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the order item");
            }
        }

        /// <summary>
        /// Gets sales statistics for a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="shopId">Shop ID (optional)</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <returns>Product sales statistics</returns>
        [HttpGet("statistics/product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductSalesStatisticsDto>> GetProductSalesStatistics(
            Guid productId,
            [FromQuery] Guid? shopId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _orderItemService.GetProductSalesStatisticsAsync(productId, shopId, startDate, endDate);
                if (statistics == null)
                {
                    return NotFound();
                }

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales statistics for product {ProductId}", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the statistics");
            }
        }

        /// <summary>
        /// Gets sales statistics for all products in a shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="startDate">Start date (optional)</param>
        /// <param name="endDate">End date (optional)</param>
        /// <param name="topProductsLimit">Limit for top selling products (optional)</param>
        /// <returns>Collection of product sales statistics</returns>
        [HttpGet("statistics/shop/{shopId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductSalesStatisticsDto>>> GetShopSalesStatistics(
            Guid shopId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? topProductsLimit = null)
        {
            try
            {
                var statistics = await _orderItemService.GetShopSalesStatisticsAsync(shopId, startDate, endDate, topProductsLimit);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales statistics for shop {ShopId}", shopId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the statistics");
            }
        }

        /// <summary>
        /// Applies a refund to an order item
        /// </summary>
        /// <param name="orderItemId">Order item ID</param>
        /// <param name="refundDto">Refund data</param>
        /// <returns>Updated order item</returns>
        [HttpPost("{orderItemId}/refund")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderItemDto>> ApplyRefund(Guid orderItemId, [FromBody] ApplyRefundDto refundDto)
        {
            try
            {
                var modifiedBy = User.Identity?.IsAuthenticated == true ?
                    User.Identity.Name : "system";

                var orderItem = await _orderItemService.ApplyRefundAsync(orderItemId, refundDto.RefundRequestId, modifiedBy);
                if (orderItem == null)
                {
                    return NotFound();
                }

                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying refund to order item {OrderItemId}", orderItemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while applying the refund");
            }
        }
    }
    public class UpdateOrderItemDto
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Notes { get; set; }
    }
    public class ApplyRefundDto
    {
        public Guid RefundRequestId { get; set; }
    }
}