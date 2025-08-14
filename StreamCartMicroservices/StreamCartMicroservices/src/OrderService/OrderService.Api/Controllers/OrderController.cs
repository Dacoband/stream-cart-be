using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.Application.Commands.OrderCommands;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.Interfaces.IServices;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using Shared.Common.Domain.Bases;
using Shared.Common.Extensions;
using Shared.Common.Models;
using Shared.Common.Services.User;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrderService.Api.Controllers
{
    [Route("api/orders")]
    [ApiController]
   // [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly Application.Interfaces.IServices.IProductServiceClient _productServiceClient;


        public OrderController(IOrderService orderService, ILogger<OrderController> logger, ICurrentUserService currentUserService, IMediator mediator, Application.Interfaces.IServices.IProductServiceClient productServiceClient)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService;
            _mediator = mediator;
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
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
            [FromQuery] FilterOrderDTO filter)
        {
            try
            {
                var orders = await _orderService.GetOrdersByAccountIdAsync(accountId, filter);
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
            [FromQuery] FilterOrderDTO filter)
        {
            try
            {
                var orders = await _orderService.GetOrdersByShopIdAsync(shopId, filter);
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

                //await _orderService.UpdateOrderStatusAsync(id, statusDto.Status, modifiedBy);
                await _mediator.Send(new UpdateOrderStatusCommand()
                {
                    OrderId = id,
                    NewStatus = statusDto.Status,
                    ModifiedBy = modifiedBy,
                });
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
                var updatedOrder = await _orderService.ConfirmOrderDeliveredAsync(orderId, customerId.ToString());
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
        [HttpPost("multi")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<OrderDto>>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateMultiOrder([FromBody] CreateMultiOrderDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid input data"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new CreateMultiOrderCommand
                {
                    AccountId = userId,
                    PaymentMethod = request.PaymentMethod,
                    LivestreamId = request.LivestreamId,
                    CreatedFromCommentId = request.CreatedFromCommentId,
                    OrdersByShop = request.OrdersByShop
                };

               
                var apiResponse = await _mediator.Send(command);
                if (apiResponse.Success == true)
                {
                    return Created($"/api/orders", ApiResponse<List<OrderDto>>.SuccessResult(apiResponse.Data, "Multiple orders created successfully"));
                }
                else return BadRequest(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating multiple orders");
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating orders: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê chi tiết đơn hàng theo cửa hàng
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <param name="fromDate">Ngày bắt đầu (tùy chọn)</param>
        /// <param name="toDate">Ngày kết thúc (tùy chọn)</param>
        /// <returns>Thống kê đơn hàng</returns>
        [HttpGet("shop/{shopId}/statistics")]
        [Authorize(Roles = "Seller,Admin,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<OrderStatisticsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetShopOrderStatistics(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync(shopId, fromDate, toDate);

                var result = new OrderStatisticsDTO
                {
                    TotalOrders = statistics.TotalOrders,
                    TotalRevenue = statistics.TotalRevenue,
                    CompleteOrderCount = statistics.OrdersByStatus.ContainsKey(OrderStatus.Delivered) ?
                        statistics.OrdersByStatus[OrderStatus.Delivered] : 0,
                    ProcessingOrderCount = (statistics.OrdersByStatus.ContainsKey(OrderStatus.Processing) ?
                        statistics.OrdersByStatus[OrderStatus.Processing] : 0) +
                        (statistics.OrdersByStatus.ContainsKey(OrderStatus.Shipped) ?
                        statistics.OrdersByStatus[OrderStatus.Shipped] : 0),
                    CanceledOrderCount = statistics.OrdersByStatus.ContainsKey(OrderStatus.Cancelled) ?
                        statistics.OrdersByStatus[OrderStatus.Cancelled] : 0,
                    RefundOrderCount = 0, // Implement once refund functionality is available
                    AverageOrderValue = statistics.AverageOrderValue,
                    PeriodStart = fromDate ?? DateTime.UtcNow.AddDays(-30),
                    PeriodEnd = toDate ?? DateTime.UtcNow
                };

                return Ok(ApiResponse<OrderStatisticsDTO>.SuccessResult(result, "Lấy thống kê đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order statistics for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }
        /// Lấy danh sách sản phẩm bán chạy nhất của cửa hàng
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <param name="fromDate">Ngày bắt đầu (tùy chọn)</param>
        /// <param name="toDate">Ngày kết thúc (tùy chọn)</param>
        /// <param name="limit">Số lượng sản phẩm tối đa trả về</param>
        /// <returns>Danh sách sản phẩm bán chạy</returns>
        [HttpGet("shop/{shopId}/top-products")]
        [Authorize(Roles = "Seller,Admin,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<TopProductsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetTopSellingProducts(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int limit = 5)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                // Get all orders for the shop within the time period
                var filter = new FilterOrderDTO
                {
                    PageIndex = 1,
                    PageSize = 1000
                };
                var ordersResult = await _orderService.GetOrdersByShopIdAsync(shopId, filter);
                var orders = ordersResult.Items
                    .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                    .Where(o => o.OrderStatus != OrderStatus.Cancelled);

                // Group orders by product, calculate sales and revenue
                var productSales = orders
                    .SelectMany(o => o.Items)
                    .GroupBy(i => i.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(i => i.Quantity),
                        Revenue = g.Sum(i => i.TotalPrice)
                    })
                    .OrderByDescending(p => p.Quantity)
                    .Take(limit)
                    .ToList();
                // Get product details
                var topProducts = new List<TopProductDTO>();
                foreach (var product in productSales)
                {
                    var productDetails = await _productServiceClient.GetProductByIdAsync(product.ProductId);
                    if (productDetails != null)
                    {
                        topProducts.Add(new TopProductDTO
                        {
                            ProductId = product.ProductId,
                            ProductName = productDetails.ProductName ?? "Unknown Product",
                            ProductImageUrl = productDetails.PrimaryImageUrl ?? string.Empty,
                            SalesCount = product.Quantity,
                            Revenue = product.Revenue
                        });
                    }
                }

                var result = new TopProductsDTO
                {
                    Products = topProducts.ToArray()
                };

                return Ok(ApiResponse<TopProductsDTO>.SuccessResult(result, "Lấy danh sách sản phẩm bán chạy thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top products for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }
        /// <summary>
        /// Lấy thống kê về khách hàng của cửa hàng
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <param name="fromDate">Ngày bắt đầu (tùy chọn)</param>
        /// <param name="toDate">Ngày kết thúc (tùy chọn)</param>
        /// <returns>Thống kê khách hàng</returns>
        [HttpGet("shop/{shopId}/customer-statistics")]
        [Authorize(Roles = "Seller,Admin,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<CustomerStatisticsDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetCustomerStatistics(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                // Get all orders for the shop within the time period
                var filter = new FilterOrderDTO
                {
                    PageIndex = 1,
                    PageSize = 1000
                };
                var ordersResult = await _orderService.GetOrdersByShopIdAsync(shopId, filter);
                var currentPeriodOrders = ordersResult.Items
                    .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                    .ToList();

                // Get all customers who ordered in this period
                var customerIds = currentPeriodOrders
                    .Select(o => o.AccountId)
                    .Distinct()
                    .ToList();

                // Get previous orders to identify returning customers
                var previousOrdersResult = await _orderService.SearchOrdersAsync(new OrderSearchParamsDto
                {
                    ShopId = shopId,
                    StartDate = null,
                    EndDate = from.AddDays(-1),
                    PageNumber = 1,
                    PageSize = 1000
                });
                var previousCustomerIds = previousOrdersResult.Items
                    .Select(o => o.AccountId)
                    .Distinct()
                    .ToList();

                // Count new vs returning customers
                var newCustomers = customerIds.Count(id => !previousCustomerIds.Contains(id));
                var returningCustomers = customerIds.Count(id => previousCustomerIds.Contains(id));

                var result = new CustomerStatisticsDTO
                {
                    NewCustomers = newCustomers,
                    RepeatCustomers = returningCustomers,
                    TotalCustomers = newCustomers + returningCustomers
                };

                return Ok(ApiResponse<CustomerStatisticsDTO>.SuccessResult(result, "Lấy thống kê khách hàng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer statistics for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }
        /// <summary>
        /// Lấy thống kê đơn hàng theo từng ngày/tuần/tháng
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <param name="fromDate">Ngày bắt đầu</param>
        /// <param name="toDate">Ngày kết thúc</param>
        /// <param name="period">Loại thời gian (daily, weekly, monthly)</param>
        /// <returns>Thống kê đơn hàng theo thời gian</returns>
        [HttpGet("shop/{shopId}/time-series")]
        [Authorize(Roles = "Seller,Admin,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<OrderTimeSeriesDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetOrderTimeSeries(
            Guid shopId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] string period = "daily")
        {
            try
            {
                // Get all orders for the shop within the time period
                var ordersResult = await _orderService.SearchOrdersAsync(new OrderSearchParamsDto
                {
                    ShopId = shopId,
                    StartDate = fromDate,
                    EndDate = toDate,
                    PageNumber = 1,
                    PageSize = 10000  // Use a large number to get all orders
                });

                var orders = ordersResult.Items.ToList();

                // Group orders by time period
                var timeSeriesData = new List<OrderTimePoint>();

                if (period.ToLower() == "daily")
                {
                    var dailyGroups = orders
                        .GroupBy(o => o.OrderDate.Date)
                        .OrderBy(g => g.Key);
                    foreach (var group in dailyGroups)
                    {
                        timeSeriesData.Add(new OrderTimePoint
                        {
                            Date = group.Key,
                            OrderCount = group.Count(),
                            Revenue = group.Sum(o => o.FinalAmount)
                        });
                    }
                }
                else if (period.ToLower() == "weekly")
                {
                    // Group by week (using ISO8601 week numbers)
                    var weeklyGroups = orders
                        .GroupBy(o => {
                            var date = o.OrderDate.Date;
                            var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
                            var weekOfYear = cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                            return new { Year = date.Year, Week = weekOfYear };
                        })
                        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week);

                    foreach (var group in weeklyGroups)
                    {
                        // Calculate the start date of the week
                        var firstOrder = group.OrderBy(o => o.OrderDate).First();
                        var weekStart = firstOrder.OrderDate.Date;

                        timeSeriesData.Add(new OrderTimePoint
                        {
                            Date = weekStart,
                            Label = $"Week {group.Key.Week}, {group.Key.Year}",
                            OrderCount = group.Count(),
                            Revenue = group.Sum(o => o.FinalAmount)
                        });
                    }
                }
                else if (period.ToLower() == "monthly")
                {
                    var monthlyGroups = orders
                        .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);

                    foreach (var group in monthlyGroups)
                    {
                        timeSeriesData.Add(new OrderTimePoint
                        {
                            Date = new DateTime(group.Key.Year, group.Key.Month, 1),
                            Label = $"{new DateTime(group.Key.Year, group.Key.Month, 1):MMM yyyy}",
                            OrderCount = group.Count(),
                            Revenue = group.Sum(o => o.FinalAmount)
                        });
                    }
                }

                var result = new OrderTimeSeriesDTO
                {
                    Period = period,
                    FromDate = fromDate,
                    ToDate = toDate,
                    DataPoints = timeSeriesData.ToArray()
                };

                return Ok(ApiResponse<OrderTimeSeriesDTO>.SuccessResult(result, "Lấy thống kê đơn hàng theo thời gian thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order time series for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }
        /// <summary>
        /// Lấy thống kê đơn hàng cho các livestream
        /// </summary>
        /// <param name="shopId">ID của cửa hàng</param>
        /// <param name="fromDate">Ngày bắt đầu (tùy chọn)</param>
        /// <param name="toDate">Ngày kết thúc (tùy chọn)</param>
        /// <returns>Thống kê đơn hàng livestream</returns>
        [HttpGet("shop/{shopId}/livestream-orders")]
        [Authorize(Roles = "Seller,Admin,OperationManager")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamOrdersDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetLivestreamOrders(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var to = toDate ?? DateTime.UtcNow;

                // Get all orders for the shop within the time period
                var ordersResult = await _orderService.SearchOrdersAsync(new OrderSearchParamsDto
                {
                    ShopId = shopId,
                    StartDate = from,
                    EndDate = to,
                    PageNumber = 1,
                    PageSize = 1000
                });

                var orders = ordersResult.Items.ToList();

                // Separate livestream orders
                var livestreamOrders = orders.Where(o => o.LivestreamId.HasValue).ToList();
                var nonLivestreamOrders = orders.Where(o => !o.LivestreamId.HasValue).ToList();
                // Group by livestream
                var livestreamStats = livestreamOrders
                    .GroupBy(o => o.LivestreamId)
                    .Select(g => new LivestreamOrderStat
                    {
                        LivestreamId = g.Key ?? Guid.Empty,
                        OrderCount = g.Count(),
                        Revenue = g.Sum(o => o.FinalAmount)
                    })
                    .OrderByDescending(s => s.OrderCount)
                    .ToArray();

                var result = new LivestreamOrdersDTO
                {
                    TotalLivestreamOrders = livestreamOrders.Count,
                    TotalNonLivestreamOrders = nonLivestreamOrders.Count,
                    LivestreamRevenue = livestreamOrders.Sum(o => o.FinalAmount),
                    NonLivestreamRevenue = nonLivestreamOrders.Sum(o => o.FinalAmount),
                    LivestreamStats = livestreamStats
                };

                return Ok(ApiResponse<LivestreamOrdersDTO>.SuccessResult(result, "Lấy thống kê đơn hàng livestream thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving livestream orders for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
        }
    }

    // DTOs for controller input
    public class OrderStatisticsDTO
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CompleteOrderCount { get; set; }
        public int ProcessingOrderCount { get; set; }
        public int CanceledOrderCount { get; set; }
        public int RefundOrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    public class TopProductDTO
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopProductsDTO
    {
        public TopProductDTO[] Products { get; set; } = Array.Empty<TopProductDTO>();
    }

    public class CustomerStatisticsDTO
    {
        public int NewCustomers { get; set; }
        public int RepeatCustomers { get; set; }
        public int TotalCustomers { get; set; }
    }
    public class OrderTimeSeriesDTO
    {
        public string Period { get; set; } = "daily";
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public OrderTimePoint[] DataPoints { get; set; } = Array.Empty<OrderTimePoint>();
    }

    public class LivestreamOrderStat
    {
        public Guid LivestreamId { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LivestreamOrdersDTO
    {
        public int TotalLivestreamOrders { get; set; }
        public int TotalNonLivestreamOrders { get; set; }
        public decimal LivestreamRevenue { get; set; }
        public decimal NonLivestreamRevenue { get; set; }
        public LivestreamOrderStat[] LivestreamStats { get; set; } = Array.Empty<LivestreamOrderStat>();
    }
    public class OrderTimePoint
    {
        public DateTime Date { get; set; }
        public string? Label { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
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