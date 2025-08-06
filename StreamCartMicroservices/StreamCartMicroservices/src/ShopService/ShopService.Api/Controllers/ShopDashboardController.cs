using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using Shared.Common.Services.User;
using ShopService.Application.Commands.Dashboard;
using ShopService.Application.DTOs.Dashboard;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Dashboard;
using ShopService.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace ShopService.Api.Controllers
{
    [ApiController]
    [Route("api/shop-dashboard")]
    [Authorize(Roles = "Seller")]
    public class ShopDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ShopDashboardController> _logger;
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly ILivestreamServiceClient _livestreamServiceClient;
        private readonly IShopManagementService _shopManagementService;

        public ShopDashboardController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<ShopDashboardController> logger,
            IOrderServiceClient orderServiceClient,
            ILivestreamServiceClient livestreamServiceClient,
            IShopManagementService shopManagementService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
            _orderServiceClient = orderServiceClient;
            _livestreamServiceClient = livestreamServiceClient;
            _shopManagementService = shopManagementService;
        }

        /// <summary>
        /// Lấy thông tin tổng quan dashboard của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <returns>Thông tin tổng quan</returns>
        [HttpGet("{shopId}/summary")]
        [ProducesResponseType(typeof(ApiResponse<ShopDashboardSummaryDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDashboardSummary(Guid shopId)
        {
            // Kiểm tra quyền truy cập
            if (!await HasShopPermission(shopId))
            {
                return Forbid();
            }

            try
            {
                var result = await _mediator.Send(new GetShopDashboardSummaryQuery { ShopId = shopId });
                return Ok(ApiResponse<ShopDashboardSummaryDTO>.SuccessResult(result, "Lấy thông tin tổng quan thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy thông tin dashboard: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết dashboard của shop theo khoảng thời gian
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="fromDate">Từ ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="toDate">Đến ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="periodType">Loại thống kê (daily, weekly, monthly, yearly)</param>
        /// <returns>Thông tin chi tiết dashboard</returns>
        [HttpGet("{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<ShopDashboardDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDashboard(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string periodType = "daily")
        {
            // Kiểm tra quyền truy cập
            if (!await HasShopPermission(shopId))
            {
                return Forbid();
            }

            try
            {
                var query = new GetShopDashboardQuery
                {
                    ShopId = shopId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    PeriodType = periodType
                };

                var result = await _mediator.Send(query);
                return Ok(ApiResponse<ShopDashboardDTO>.SuccessResult(result, "Lấy thông tin dashboard thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy thông tin dashboard: {ex.Message}"));
            }
        }

        /// <summary>
        /// Tạo mới/cập nhật dashboard của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="request">Thông tin yêu cầu</param>
        /// <returns>Dashboard đã tạo/cập nhật</returns>
        [HttpPost("{shopId}/generate")]
        [ProducesResponseType(typeof(ApiResponse<ShopDashboardDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GenerateDashboard(Guid shopId, [FromBody] GetShopDashboardRequestDTO request)
        {
            // Kiểm tra quyền truy cập
            //if (!await HasShopPermission(shopId))
            //{
            //    return Forbid();
            //}

            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new GenerateDashboardCommand
                {
                    ShopId = shopId,
                    FromDate = request.FromDate ?? DateTime.UtcNow.Date.AddDays(-30),
                    ToDate = request.ToDate ?? DateTime.UtcNow,
                    PeriodType = request.PeriodType ?? "daily",
                    GeneratedBy = userId.ToString()
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<ShopDashboardDTO>.SuccessResult(result, "Tạo dashboard thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi tạo dashboard: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật ghi chú cho dashboard
        /// </summary>
        /// <param name="dashboardId">ID của dashboard</param>
        /// <param name="notes">Nội dung ghi chú</param>
        /// <returns>Dashboard đã cập nhật</returns>
        [HttpPatch("{dashboardId}/notes")]
        [ProducesResponseType(typeof(ApiResponse<ShopDashboardDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDashboardNotes(Guid dashboardId, [FromBody] UpdateDashboardNotesDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new UpdateDashboardNotesCommand
                {
                    DashboardId = dashboardId,
                    Notes = request.Notes,
                    UpdatedBy = userId.ToString()
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<ShopDashboardDTO>.SuccessResult(result, "Cập nhật ghi chú thành công"));
            }
            catch (ArgumentException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notes for dashboard {DashboardId}", dashboardId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi cập nhật ghi chú: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê đơn hàng của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="fromDate">Từ ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="toDate">Đến ngày (định dạng yyyy-MM-dd)</param>
        /// <returns>Thống kê đơn hàng</returns>
        [HttpGet("{shopId}/order-statistics")]
        [ProducesResponseType(typeof(ApiResponse<OrderStatisticsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetOrderStatistics(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            // Kiểm tra quyền truy cập
            if (!await HasShopPermission(shopId))
            {
                return Forbid();
            }

            try
            {
                var result = await _orderServiceClient.GetOrderStatisticsAsync(
                    shopId,
                    fromDate ?? DateTime.UtcNow.Date.AddDays(-30),
                    toDate ?? DateTime.UtcNow);

                return Ok(ApiResponse<OrderStatisticsDTO>.SuccessResult(result, "Lấy thống kê đơn hàng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy thống kê đơn hàng: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê livestream của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="fromDate">Từ ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="toDate">Đến ngày (định dạng yyyy-MM-dd)</param>
        /// <returns>Thống kê livestream</returns>
        [HttpGet("{shopId}/livestream-statistics")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamStatisticsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetLivestreamStatistics(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            // Kiểm tra quyền truy cập
            if (!await HasShopPermission(shopId))
            {
                return Forbid();
            }

            try
            {
                var result = await _livestreamServiceClient.GetLivestreamStatisticsAsync(
            shopId,
            fromDate ?? DateTime.UtcNow.Date.AddDays(-30),
            toDate ?? DateTime.UtcNow);

                return Ok(ApiResponse<LivestreamStatisticsDTO>.SuccessResult(result, "Lấy thống kê livestream thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream statistics for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy thống kê livestream: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm bán chạy nhất của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="fromDate">Từ ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="toDate">Đến ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="limit">Số lượng sản phẩm trả về</param>
        /// <returns>Danh sách sản phẩm bán chạy</returns>
        [HttpGet("{shopId}/top-products")]
        [ProducesResponseType(typeof(ApiResponse<TopProductsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTopProducts(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int limit = 5)
        {
            // Kiểm tra quyền truy cập
            if (!await HasShopPermission(shopId))
            {
                return Forbid();
            }

            try
            {
                var result = await _orderServiceClient.GetTopSellingProductsAsync(
                    shopId,
                    fromDate ?? DateTime.UtcNow.Date.AddDays(-30),
                    toDate ?? DateTime.UtcNow,
                    limit);

                return Ok(ApiResponse<TopProductsDTO>.SuccessResult(result, "Lấy danh sách sản phẩm bán chạy thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top products for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy danh sách sản phẩm bán chạy: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê khách hàng của shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="fromDate">Từ ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="toDate">Đến ngày (định dạng yyyy-MM-dd)</param>
        /// <returns>Thống kê khách hàng</returns>
        [HttpGet("{shopId}/customer-statistics")]
        [ProducesResponseType(typeof(ApiResponse<CustomerStatisticsDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCustomerStatistics(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            // Kiểm tra quyền truy cập
            if (!await HasShopPermission(shopId))
            {
                return Forbid();
            }

            try
            {
                var result = await _orderServiceClient.GetCustomerStatisticsAsync(
                    shopId,
                    fromDate ?? DateTime.UtcNow.Date.AddDays(-30),
                    toDate ?? DateTime.UtcNow);

                return Ok(ApiResponse<CustomerStatisticsDTO>.SuccessResult(result, "Lấy thống kê khách hàng thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy thống kê khách hàng: {ex.Message}"));
            }
        }

        // Helper method to check shop permission
        private async Task<bool> HasShopPermission(Guid shopId)
        {
            var userId = _currentUserService.GetUserId();

            // Nếu là admin hoặc operation manager, luôn có quyền
            if (_currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("OperationManager"))
            {
                return true;
            }

            // Kiểm tra người dùng có quyền với shop không
            return await _shopManagementService.HasShopPermissionAsync(shopId, userId, "Owner,Manager");

        }
        /// <summary>
        /// Lấy dữ liệu theo dòng thời gian của đơn hàng
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="fromDate">Từ ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="toDate">Đến ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="period">Khoảng thời gian (daily, weekly, monthly)</param>
        /// <returns>Dữ liệu theo dòng thời gian</returns>
        [HttpGet("{shopId}/order-time-series")]
        [ProducesResponseType(typeof(ApiResponse<OrderTimeSeriesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetOrderTimeSeries(
            Guid shopId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] string period = "daily")
        {
            // Kiểm tra quyền truy cập
            if (!await HasShopPermission(shopId))
            {
                return Forbid();
            }

            try
            {
                 var result = await _orderServiceClient.GetOrderTimeSeriesAsync(
                    shopId,
                    fromDate,
                    toDate,
                    period);

                return Ok(ApiResponse<OrderTimeSeriesDTO>.SuccessResult(result, "Lấy dữ liệu đơn hàng theo thời gian thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order time series for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy dữ liệu đơn hàng theo thời gian: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy thống kê đơn hàng từ livestream
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="fromDate">Từ ngày (định dạng yyyy-MM-dd)</param>
        /// <param name="toDate">Đến ngày (định dạng yyyy-MM-dd)</param>
        /// <returns>Thống kê đơn hàng từ livestream</returns>
        [HttpGet("{shopId}/livestream-orders")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamOrdersDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetLivestreamOrders(
            Guid shopId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            // Kiểm tra quyền truy cập
            if (!await HasShopPermission(shopId))
            {
                return Forbid();
            }

            try
            {
                 var result = await _orderServiceClient.GetLivestreamOrdersAsync(
                    shopId,
                    fromDate ?? DateTime.UtcNow.Date.AddDays(-30),
                    toDate ?? DateTime.UtcNow);

                return Ok(ApiResponse<LivestreamOrdersDTO>.SuccessResult(result, "Lấy thống kê đơn hàng từ livestream thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting livestream orders for shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy thống kê đơn hàng từ livestream: {ex.Message}"));
            }
        }
    }
}