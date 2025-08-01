using MassTransit.Internals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Services;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentService.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IQrCodeService _qrCodeService;
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentsController(IPaymentService paymentService, IQrCodeService qrCodeService, IOrderServiceClient orderServiceClient, ILogger<PaymentsController> logger, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _qrCodeService = qrCodeService;
            _orderServiceClient = orderServiceClient;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Tạo payment mới
        /// </summary>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set creator from JWT claim if not provided
            if (string.IsNullOrEmpty(createPaymentDto.CreatedBy))
            {
                createPaymentDto.CreatedBy = User.Identity?.Name ?? "Anonymous";
            }

            var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);
            return CreatedAtAction(nameof(GetPayment), new { paymentId = payment.Id }, payment);
        }

        /// <summary>
        /// Lấy thông tin chi tiết payment
        /// </summary>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPayment(Guid paymentId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        /// <summary>
        /// Lấy danh sách payment theo các tiêu chí
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<PaymentDto>>> GetPayments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] PaymentStatus? status = null,
            [FromQuery] PaymentMethod? method = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] Guid? orderId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool ascending = true)
        {
            var payments = await _paymentService.GetPagedPaymentsAsync(
                pageNumber, pageSize, status, method, userId, orderId, fromDate, toDate, sortBy, ascending);

            return Ok(payments);
        }

        /// <summary>
        /// Cập nhật trạng thái payment
        /// </summary>
        [HttpPatch("{paymentId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> UpdatePaymentStatus(
            Guid paymentId, [FromBody] Application.DTOs.UpdatePaymentStatusDto updateStatusDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set updater from JWT claim if not provided
            if (string.IsNullOrEmpty(updateStatusDto.UpdatedBy))
            {
                updateStatusDto.UpdatedBy = User.Identity?.Name ?? "Anonymous";
            }

            var payment = await _paymentService.UpdatePaymentStatusAsync(paymentId, updateStatusDto);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        /// <summary>
        /// Xử lý callback/cập nhật từ cổng thanh toán
        /// </summary>
        [HttpPost("callbacks/{paymentId}")]
        [AllowAnonymous] // Cổng thanh toán không có JWT
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> ProcessPaymentCallback(
            Guid paymentId, [FromBody] PaymentCallbackDto callbackDto)
        {
            var payment = await _paymentService.ProcessPaymentCallbackAsync(paymentId, callbackDto);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        /// <summary>
        /// Yêu cầu hoàn tiền
        /// </summary>
        [HttpPost("refunds/{paymentId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> RefundPayment(
            Guid paymentId, [FromBody] RefundPaymentDto refundDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set requester from JWT claim if not provided
            if (string.IsNullOrEmpty(refundDto.RequestedBy))
            {
                refundDto.RequestedBy = User.Identity?.Name ?? "Anonymous";
            }

            var payment = await _paymentService.RefundPaymentAsync(paymentId, refundDto);
            if (payment == null)
                return NotFound();

            return Ok(payment);
        }

        /// <summary>
        /// Lấy lịch sử thanh toán theo user
        /// </summary>
        [HttpGet("users/{userId}/transactions")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByUserId(Guid userId)
        {
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(payments);
        }

        /// <summary>
        /// Lấy thanh toán của đơn hàng
        /// </summary>
        [HttpGet("orders/{orderId}/payments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByOrderId(Guid orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
            return Ok(payments);
        }

        /// <summary>
        /// Xóa payment
        /// </summary>
        [HttpDelete("{paymentId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePayment(Guid paymentId)
        {
            var deletedBy = User.Identity?.Name ?? "Anonymous";
            var result = await _paymentService.DeletePaymentAsync(paymentId, deletedBy);

            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Thống kê payment
        /// </summary>
        [HttpGet("summary")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaymentSummaryDto>> GetPaymentSummary(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var summary = await _paymentService.GetPaymentSummaryAsync(fromDate, toDate);
            return Ok(summary);
        }
        [HttpPost("generate-qr-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GenerateBulkQrCode([FromBody] QrCodeRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate request data
            if (requestDto.OrderIds == null || requestDto.OrderIds.Count == 0)
            {
                return BadRequest("At least one order ID must be provided");
            }

            try
            {
                // Get all orders
                decimal totalAmount = 0;
                Guid? primaryUserId = null;
                var orderDetails = new List<OrderDto>();

                foreach (var orderId in requestDto.OrderIds)
                {
                    var order = await _orderServiceClient.GetOrderByIdAsync(orderId);
                    if (order == null)
                        continue;

                    // Add to total amount
                    totalAmount += order.TotalAmount;
                    orderDetails.Add(order);

                    // Use first order's user ID
                    if (!primaryUserId.HasValue)
                        primaryUserId = order.UserId;
                }

                if (orderDetails.Count == 0)
                    return NotFound("No valid orders found");

                // Generate QR code for combined amount with ALL order IDs
                string qrCode;
                if (requestDto.OrderIds.Count == 1)
                {
                    // Single order - use original method
                    qrCode = await _qrCodeService.GenerateQrCodeAsync(
                        requestDto.OrderIds[0],
                        totalAmount,
                        primaryUserId.Value,
                        PaymentMethod.BankTransfer
                    );
                }
                else
                {
                    // Multiple orders - use bulk method with ALL order IDs
                    qrCode = await _qrCodeService.GenerateBulkQrCodeAsync(
                        requestDto.OrderIds,
                        totalAmount,
                        primaryUserId.Value,
                        PaymentMethod.BankTransfer
                    );
                }

                // Create combined payment record
                var createPaymentDto = new CreatePaymentDto
                {
                    OrderId = orderDetails[0].Id, // Primary order ID
                    Amount = totalAmount,
                    PaymentMethod = PaymentMethod.BankTransfer,
                    CreatedBy = User.Identity?.Name ?? "System",
                    QrCode = qrCode,
                    OrderReference = string.Join(",", requestDto.OrderIds) 
                };

                var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);

                // Update all orders' payment status to pending
                foreach (var order in orderDetails)
                {
                    await _orderServiceClient.UpdateOrderPaymentStatusAsync(
                        order.Id, PaymentStatus.Pending);
                }

                // Return QR code and payment information
                return Ok(new
                {
                    qrCode = qrCode,
                    paymentId = payment.Id,
                    totalAmount = totalAmount,
                    orderCount = orderDetails.Count,
                    orderIds = requestDto.OrderIds,
                    description = $"ORDERS_{string.Join(",", requestDto.OrderIds.Select(id => id.ToString("N")))}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bulk QR code for orders");
                return StatusCode(500, new { error = $"Error generating QR code: {ex.Message}" });
            }
        }
        /// <summary>
        /// Xử lý callback từ SePay
        /// </summary>
        /// <summary>
        /// Xử lý callback từ SePay với chuyển hướng về frontend
        /// </summary>
        /// <summary>
        /// Xử lý callback từ SePay với chuyển hướng về frontend
        /// </summary>
        /// <summary>
        /// Xử lý callback từ SePay với chuyển hướng về frontend
        /// </summary>
        [HttpPost("callback/sepay")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> ProcessSePayCallback([FromBody] SePayCallbackRequest request)
        {
            try
            {
                // Trích xuất thông tin từ request của SePay
                var transactionId = request.TransactionId;
                var orderCode = request.OrderCode;
                var amount = request.Amount;
                var status = request.Status;

                // Tìm payment dựa trên mã đơn hàng
                if (string.IsNullOrEmpty(orderCode))
                {
                    return BadRequest("Order code is null or empty");
                }

                // Handle both single and bulk orders
                List<Guid> orderIds = new List<Guid>();

                if (orderCode.StartsWith("ORDERS_"))
                {
                    // Multiple orders format: ORDERS_id1,id2,id3
                    var orderIdsString = orderCode.Replace("ORDERS_", "");
                    orderIds = orderIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => Guid.Parse(id.Trim()))
                        .ToList();
                }
                else if (orderCode.StartsWith("ORDER_"))
                {
                    // Single order format: ORDER_id
                    var orderId = Guid.Parse(orderCode.Replace("ORDER_", ""));
                    orderIds.Add(orderId);
                }
                else
                {
                    return BadRequest("Invalid order code format");
                }

                // Find payment by first order ID (primary order)
                var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderIds.First());

                if (payments == null || !payments.Any())
                {
                    return BadRequest("Payment not found");
                }

                var payment = payments.FirstOrDefault();

                // Ensure payment.QrCode is not null before accessing it
                if (payment?.QrCode == null)
                {
                    return BadRequest("QR Code not found for the payment");
                }

                // Tạo callback dto từ dữ liệu của SePay
                var callbackDto = new PaymentCallbackDto
                {
                    IsSuccessful = status == "success",
                    QrCode = payment.QrCode,
                    RawResponse = System.Text.Json.JsonSerializer.Serialize(request)
                };

                // Xử lý callback
                var result = await _paymentService.ProcessPaymentCallbackAsync(payment.Id, callbackDto);

                // LUÔN REDIRECT về frontend (bỏ check header)
                // Lấy URL base từ cấu hình
                string baseRedirectUrl = status == "success"
                    ? _configuration["PaymentRedirects:SuccessUrl"]
                    : _configuration["PaymentRedirects:FailureUrl"];

                if (string.IsNullOrEmpty(baseRedirectUrl))
                {
                    // Fallback URL nếu không cấu hình
                    baseRedirectUrl = status == "success"
                        ? "http://localhost:3000/payment/order/results-success/"
                        : "http://localhost:3000/payment/order/results-failed/";
                }

                // Build redirect URL với order IDs
                string redirectUrl;
                if (orderIds.Count == 1)
                {
                    // Single order: append order ID directly to URL
                    redirectUrl = $"{baseRedirectUrl.TrimEnd('/')}/{orderIds.First()}";
                }
                else
                {
                    // Multiple orders: use comma-separated list as path parameter
                    var orderIdsString = string.Join(",", orderIds);
                    redirectUrl = $"{baseRedirectUrl.TrimEnd('/')}/{orderIdsString}";
                }

                _logger.LogInformation("Redirecting to: {RedirectUrl}", redirectUrl);

                // Chuyển hướng về frontend (KHÔNG có query parameters)
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay callback");

                // Trong trường hợp lỗi, redirect về trang failed với error message
                string errorRedirectUrl = _configuration["PaymentRedirects:FailureUrl"] ??
                    "http://localhost:3000/payment/order/results-failed/";

                return Redirect($"{errorRedirectUrl.TrimEnd('/')}/error");
            }
        }
    }
}