using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Common.Domain.Bases;
using Shared.Common.Services.User;
using System;
using System.Threading.Tasks;

namespace OrderService.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger, ICurrentUserService currentUserService)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="orderDto">Order creation data</param>
        /// <returns>Created order</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            try
            {
                // Validate the order
                var (isValid, errorMessage) = await _orderService.ValidateOrderAsync(orderDto);
                if (!isValid)
                {
                    return BadRequest(errorMessage);
                }

                var order = await _orderService.CreateOrderAsync(orderDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the order");
            }
        }

        /// <summary>
        /// Gets an order by ID
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <returns>Order details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with ID {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order");
            }
        }

        /// <summary>
        /// Gets an order by order code
        /// </summary>
        /// <param name="code">Order code</param>
        /// <returns>Order details</returns>
        [HttpGet("code/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> GetOrderByCode(string code)
        {
            try
            {
                var order = await _orderService.GetOrderByCodeAsync(code);
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order with code {OrderCode}", code);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order");
            }
        }

        /// <summary>
        /// Gets orders for an account
        /// </summary>
        /// <param name="accountId">Account ID</param>
        /// <param name="pageNumber">Page number (optional)</param>
        /// <param name="pageSize">Page size (optional)</param>
        /// <returns>Paged result of orders</returns>
        [HttpGet("account/{accountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetOrdersByAccountId(
            Guid accountId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetOrdersByAccountIdAsync(accountId, pageNumber, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for account {AccountId}", accountId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the orders");
            }
        }

        /// <summary>
        /// Gets orders for a shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="pageNumber">Page number (optional)</param>
        /// <param name="pageSize">Page size (optional)</param>
        /// <returns>Paged result of orders</returns>
        [HttpGet("shop/{shopId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetOrdersByShopId(
            Guid shopId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var orders = await _orderService.GetOrdersByShopIdAsync(shopId, pageNumber, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for shop {ShopId}", shopId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the orders");
            }
        }

        /// <summary>
        /// Searches for orders based on various criteria
        /// </summary>
        /// <param name="searchParams">Search parameters</param>
        /// <returns>Paged result of matching orders</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<OrderDto>>> SearchOrders([FromQuery] OrderSearchParamsDto searchParams)
        {
            try
            {
                var orders = await _orderService.SearchOrdersAsync(searchParams);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching orders");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while searching for orders");
            }
        }

        /// <summary>
        /// Updates an order's status
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="statusDto">Status update data</param>
        /// <returns>Updated order</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                // Get current user ID from authentication as modifier
                var modifiedBy = User.Identity?.IsAuthenticated == true ?
                    User.Identity.Name : "system";
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound();

                await _orderService.UpdateOrderStatusAsync(id, statusDto.Status, modifiedBy);
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order status");
            }
        }

        /// <summary>
        /// Updates an order's payment status
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="paymentStatusDto">Payment status update data</param>
        /// <returns>Updated order</returns>
        [HttpPut("{id}/payment-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> UpdatePaymentStatus(Guid id, [FromBody] UpdatePaymentStatusDto paymentStatusDto)
        {
            try
            {
                // Get current user ID from authentication as modifier
                var modifiedBy = User.Identity?.IsAuthenticated == true ?
                    User.Identity.Name : "system";

                var order = await _orderService.UpdatePaymentStatusAsync(id, paymentStatusDto.Status, modifiedBy);
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for order {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the payment status");
            }
        }

        /// <summary>
        /// Updates an order's tracking code
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="trackingCodeDto">Tracking code update data</param>
        /// <returns>Updated order</returns>
        [HttpPut("{id}/tracking-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> UpdateTrackingCode(Guid id, [FromBody] UpdateTrackingCodeDto trackingCodeDto)
        {
            try
            {
                var modifiedBy = User.Identity?.IsAuthenticated == true ?
                    User.Identity.Name : "system";

                var order = await _orderService.UpdateTrackingCodeAsync(id, trackingCodeDto.TrackingCode, modifiedBy);
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tracking code for order {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the tracking code");
            }
        }

        /// <summary>
        /// Updates an order's shipping information
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="shippingInfoDto">Shipping info update data</param>
        /// <returns>Updated order</returns>
        [HttpPut("{id}/shipping-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> UpdateShippingInfo(Guid id, [FromBody] UpdateShippingInfoDto shippingInfoDto)
        {
            try
            {
                var modifiedBy = User.Identity?.IsAuthenticated == true ?
                    User.Identity.Name : "system";

                var order = await _orderService.UpdateShippingInfoAsync(
                    id,
                    shippingInfoDto.ShippingAddress,
                    shippingInfoDto.ShippingMethod,
                    shippingInfoDto.ShippingFee,
                    modifiedBy);

                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping info for order {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the shipping information");
            }
        }

        /// <summary>
        /// Cancels an order
        /// </summary>
        /// <param name="id">Order ID</param>
        /// <param name="cancelDto">Cancellation data</param>
        /// <returns>Cancelled order</returns>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderDto>> CancelOrder(Guid id, [FromBody] CancelOrderDto cancelDto)
        {
            try
            {
                var cancelledBy = User.Identity?.IsAuthenticated == true ?
                    User.Identity.Name : "system";

                var order = await _orderService.CancelOrderAsync(id, cancelDto.CancelReason, cancelledBy);
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while cancelling the order");
            }
        }

        /// <summary>
        /// Gets order statistics for a shop
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <returns>Order statistics</returns>
        [HttpGet("statistics/{shopId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OrderStatisticsDto>> GetOrderStatistics(
            Guid shopId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync(shopId, startDate, endDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for shop {ShopId}", shopId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the statistics");
            }
        }
        /// <summary>
        /// Khách hàng xác nhận đã nhận được hàng
        /// </summary>
        /// <param name="orderId">ID của đơn hàng</param>
        /// <returns>Thông tin đơn hàng đã cập nhật</returns>
        [HttpPost("{orderId}/confirm-delivery")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> ConfirmOrderDelivery(Guid orderId)
        {
            try
            {
                // Lấy ID của người dùng hiện tại
                var customerId = _currentUserService.GetUserId();
                if (customerId == Guid.Empty)
                {
                    _logger.LogWarning("Không thể xác nhận đơn hàng: Người dùng chưa đăng nhập");
                    return Unauthorized(new { error = "Bạn cần đăng nhập để xác nhận đơn hàng" });
                }

                // Gọi service để xác nhận đơn hàng
                var updatedOrder = await _orderService.ConfirmOrderDeliveredAsync(orderId, customerId);
                if (updatedOrder == null)
                {
                    _logger.LogWarning("Không thể xác nhận đơn hàng {OrderId}: Đơn hàng không tồn tại hoặc không thuộc về khách hàng", orderId);
                    return NotFound(new { error = "Không tìm thấy đơn hàng hoặc bạn không có quyền xác nhận đơn hàng này" });
                }

                // Trả về kết quả thành công
                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi khách hàng xác nhận đơn hàng {OrderId}", orderId);
                return BadRequest(new { error = "Có lỗi xảy ra khi xác nhận đơn hàng. Vui lòng thử lại sau." });
            }
        }
    }

    // DTOs for controller input

    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }

    public class UpdatePaymentStatusDto
    {
        public PaymentStatus Status { get; set; }
    }

    public class UpdateTrackingCodeDto
    {
        public string TrackingCode { get; set; }
    }

    public class UpdateShippingInfoDto
    {
        public ShippingAddressDto ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public decimal? ShippingFee { get; set; }
    }

    public class CancelOrderDto
    {
        public string CancelReason { get; set; }
    }
}