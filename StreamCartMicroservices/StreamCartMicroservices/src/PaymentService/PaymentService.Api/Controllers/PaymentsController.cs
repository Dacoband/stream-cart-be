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
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        private readonly IWalletServiceClient _walletServiceClient;
        private readonly ICurrentUserService _currentUserService;

        public PaymentsController(IPaymentService paymentService, IQrCodeService qrCodeService, IOrderServiceClient orderServiceClient, ILogger<PaymentsController> logger, IConfiguration configuration,IWalletServiceClient walletServiceClient,ICurrentUserService currentUserService)
        {
            _paymentService = paymentService;
            _qrCodeService = qrCodeService;
            _orderServiceClient = orderServiceClient;
            _logger = logger;
            _configuration = configuration;
            _walletServiceClient = walletServiceClient;
            _currentUserService = currentUserService;
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

            
            if (requestDto.OrderIds == null || requestDto.OrderIds.Count == 0)
            {
                return BadRequest("At least one order ID must be provided");
            }

            try
            {
               
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

                var createPaymentDto = new CreatePaymentDto
                {
                    OrderId = orderDetails[0].Id, 
                    Amount = totalAmount,
                    PaymentMethod = PaymentMethod.BankTransfer,
                    CreatedBy = User.Identity?.Name ?? "System",
                    QrCode = qrCode,
                    //OrderReference = string.Join(",", requestDto.OrderIds) 
                };

                var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);

                foreach (var order in orderDetails)
                {
                    await _orderServiceClient.UpdateOrderPaymentStatusAsync(
                        order.Id, PaymentStatus.Pending);
                }

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

        [HttpPost("callback/sepay")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public async Task<IActionResult> ProcessSePayCallback([FromBody] SePayCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received SePay callback: {@Request}", request);

                // Trích xuất thông tin từ request của SePay
                var transactionId = request.TransactionId;
                var orderCode = request.OrderCode;
                var amount = request.Amount;
                var status = request.Status;

                _logger.LogInformation("Extracted - TransactionId: {TransactionId}, OrderCode: {OrderCode}, Amount: {Amount}, Status: {Status}",
                    transactionId, orderCode, amount, status);

                // Tìm payment dựa trên mã đơn hàng
                if (string.IsNullOrEmpty(orderCode))
                {
                    _logger.LogWarning("Could not extract order code from content: {Content}", request.Content);
                    return BadRequest("Order code could not be extracted from payment content");
                }

                // Handle both single and bulk orders
                List<Guid> orderIds = new List<Guid>();

                try
                {
                    if (orderCode.StartsWith("ORDERS_", StringComparison.OrdinalIgnoreCase))
                    {
                        // Multiple orders format: ORDERS_id1,id2,id3
                        var orderIdsString = orderCode.Substring(7); // Remove "ORDERS_"
                        orderIds = orderIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => Guid.Parse(id.Trim()))
                            .ToList();
                    }
                    else if (orderCode.StartsWith("ORDER_", StringComparison.OrdinalIgnoreCase))
                    {
                        // Single order format: ORDER_id
                        var orderIdString = orderCode.Substring(6); // Remove "ORDER_"
                        orderIds.Add(Guid.Parse(orderIdString));
                    }
                    else if (orderCode.StartsWith("ORDER", StringComparison.OrdinalIgnoreCase))
                    {
                        // Handle case without underscore: ORDERxxxxx
                        var orderIdString = orderCode.Substring(5); // Remove "ORDER"

                        // Tìm đoạn GUID hợp lệ trong chuỗi
                        var guidPattern = @"[0-9a-fA-F]{32}";
                        var match = System.Text.RegularExpressions.Regex.Match(orderIdString, guidPattern);

                        if (match.Success)
                        {
                            var guidString = match.Value;
                            // Chuyển đổi từ 32 ký tự thành GUID format
                            var formattedGuid = $"{guidString.Substring(0, 8)}-{guidString.Substring(8, 4)}-{guidString.Substring(12, 4)}-{guidString.Substring(16, 4)}-{guidString.Substring(20, 12)}";
                            orderIds.Add(Guid.Parse(formattedGuid));
                        }
                        else
                        {
                            throw new FormatException($"Could not extract valid GUID from order code: {orderCode}");
                        }
                    }
                    else
                    {
                        throw new FormatException($"Invalid order code format: {orderCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing order code: {OrderCode}", orderCode);
                    return BadRequest($"Invalid order code format: {orderCode}");
                }

                if (!orderIds.Any())
                {
                    _logger.LogWarning("No valid order IDs extracted from order code: {OrderCode}", orderCode);
                    return BadRequest("No valid order IDs found");
                }

                _logger.LogInformation("Extracted order IDs: {OrderIds}", string.Join(", ", orderIds));

                // Find payment by first order ID (primary order)
                var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderIds.First());

                if (payments == null || !payments.Any())
                {
                    _logger.LogWarning("Payment not found for order ID: {OrderId}", orderIds.First());
                    return BadRequest("Payment not found");
                }

                var payment = payments.FirstOrDefault();

                // Ensure payment.QrCode is not null before accessing it
                if (payment?.QrCode == null)
                {
                    _logger.LogWarning("QR Code not found for payment: {PaymentId}", payment?.Id);
                    return BadRequest("QR Code not found for the payment");
                }

                // Tạo callback dto từ dữ liệu của SePay
                var callbackDto = new PaymentCallbackDto
                {
                    IsSuccessful = status == "success",
                    QrCode = payment.QrCode,
                    RawResponse = System.Text.Json.JsonSerializer.Serialize(request)
                };

                _logger.LogInformation("Processing payment callback - PaymentId: {PaymentId}, IsSuccessful: {IsSuccessful}",
                    payment.Id, callbackDto.IsSuccessful);

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
                        ? "https://stream-cart-fe.vercel.app/payment/order/results-success"
                        : "https://stream-cart-fe.vercel.app/payment/order/results-failed";
                }
                // Build redirect URL với order IDs
                //string redirectUrl;
                //if (orderIds.Count == 1)
                //{
                //    // Single order: append order ID directly to URL
                //    redirectUrl = $"{baseRedirectUrl.TrimEnd('/')}/{orderIds.First()}";
                //}
                //else
                //{
                //    // Multiple orders: use comma-separated list as path parameter
                //    var orderIdsString = string.Join(",", orderIds);
                //    redirectUrl = $"{baseRedirectUrl.TrimEnd('/')}/{orderIdsString}";
                //}
                // Build redirect URL với order IDs
                string redirectUrl;
                if (orderIds.Count == 1)
                {
                    // Single order: dùng query string với 1 id
                    redirectUrl = $"{baseRedirectUrl.TrimEnd('/')}?orders={orderIds.First()}";
                }
                else
                {
                    // Multiple orders: dùng query string với nhiều id, phân cách bằng dấu phẩy
                    var orderIdsString = string.Join(",", orderIds);
                    redirectUrl = $"{baseRedirectUrl.TrimEnd('/')}?orders={orderIdsString}";
                }


                _logger.LogInformation("Redirecting to: {RedirectUrl}", redirectUrl);

                // Chuyển hướng về frontend (KHÔNG có query parameters)
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay callback: {@Request}", request);

                // Trong trường hợp lỗi, redirect về trang failed với error message
                string errorRedirectUrl = _configuration["PaymentRedirects:FailureUrl"] ??
                    "https://stream-cart-fe.vercel.app/payment/order/results-failed";

                return Redirect($"{errorRedirectUrl.TrimEnd('/')}/error");
            }
        }
        /// <summary>
        /// Xử lý callback từ SePay (phiên bản API trả về JSON thay vì redirect)
        /// </summary>
        [HttpPost("callback/sepay-api")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessSePayCallbackApi([FromBody] SePayCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received SePay callback API request: {@Request}", request);

                // Trích xuất thông tin từ request của SePay
                var transactionId = request.TransactionId;
                var orderCode = request.OrderCode;
                var amount = request.Amount;
                var status = request.Status;

                _logger.LogInformation("Extracted - TransactionId: {TransactionId}, OrderCode: {OrderCode}, Amount: {Amount}, Status: {Status}",
                    transactionId, orderCode, amount, status);

                // Tìm payment dựa trên mã đơn hàng
                if (string.IsNullOrEmpty(orderCode))
                {
                    _logger.LogWarning("Could not extract order code from content: {Content}", request.Content);
                    return BadRequest(new { success = false, error = "Order code could not be extracted from payment content" });
                }

                // Handle both single and bulk orders
                List<Guid> orderIds = new List<Guid>();

                try
                {
                    if (orderCode.StartsWith("ORDERS_", StringComparison.OrdinalIgnoreCase))
                    {
                        // Multiple orders format: ORDERS_id1,id2,id3
                        var orderIdsString = orderCode.Substring(7); // Remove "ORDERS_"
                        orderIds = orderIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(id => Guid.Parse(id.Trim()))
                            .ToList();
                    }
                    else if (orderCode.StartsWith("ORDER_", StringComparison.OrdinalIgnoreCase))
                    {
                        // Single order format: ORDER_id
                        var orderIdString = orderCode.Substring(6); // Remove "ORDER_"
                        orderIds.Add(Guid.Parse(orderIdString));
                    }
                    else if (orderCode.StartsWith("ORDER", StringComparison.OrdinalIgnoreCase))
                    {
                        // Handle case without underscore: ORDERxxxxx
                        var orderIdString = orderCode.Substring(5); // Remove "ORDER"

                        // Tìm đoạn GUID hợp lệ trong chuỗi
                        var guidPattern = @"[0-9a-fA-F]{32}";
                        var match = System.Text.RegularExpressions.Regex.Match(orderIdString, guidPattern);

                        if (match.Success)
                        {
                            var guidString = match.Value;
                            // Chuyển đổi từ 32 ký tự thành GUID format
                            var formattedGuid = $"{guidString.Substring(0, 8)}-{guidString.Substring(8, 4)}-{guidString.Substring(12, 4)}-{guidString.Substring(16, 4)}-{guidString.Substring(20, 12)}";
                            orderIds.Add(Guid.Parse(formattedGuid));
                        }
                        else
                        {
                            throw new FormatException($"Could not extract valid GUID from order code: {orderCode}");
                        }
                    }
                    else
                    {
                        throw new FormatException($"Invalid order code format: {orderCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing order code: {OrderCode}", orderCode);
                    return BadRequest(new { success = false, error = $"Invalid order code format: {orderCode}" });
                }

                if (!orderIds.Any())
                {
                    _logger.LogWarning("No valid order IDs extracted from order code: {OrderCode}", orderCode);
                    return BadRequest(new { success = false, error = "No valid order IDs found" });
                }

                _logger.LogInformation("Extracted order IDs: {OrderIds}", string.Join(", ", orderIds));

                // Find payment by first order ID (primary order)
                var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderIds.First());

                if (payments == null || !payments.Any())
                {
                    _logger.LogWarning("Payment not found for order ID: {OrderId}", orderIds.First());
                    return BadRequest(new { success = false, error = "Payment not found" });
                }

                var payment = payments.FirstOrDefault();

                // Ensure payment.QrCode is not null before accessing it
                if (payment?.QrCode == null)
                {
                    _logger.LogWarning("QR Code not found for payment: {PaymentId}", payment?.Id);
                    return BadRequest(new { success = false, error = "QR Code not found for the payment" });
                }

                // Tạo callback dto từ dữ liệu của SePay
                var callbackDto = new PaymentCallbackDto
                {
                    IsSuccessful = status == "success",
                    QrCode = payment.QrCode,
                    RawResponse = System.Text.Json.JsonSerializer.Serialize(request)
                };

                _logger.LogInformation("Processing payment callback - PaymentId: {PaymentId}, IsSuccessful: {IsSuccessful}",
                    payment.Id, callbackDto.IsSuccessful);

                // Xử lý callback
                var result = await _paymentService.ProcessPaymentCallbackAsync(payment.Id, callbackDto);

                // Trả về JSON response thay vì redirect
                return Ok(new
                {
                    success = true,
                    message = "Payment processed successfully",
                    paymentId = payment.Id,
                    orderIds = orderIds,
                    status = status == "success" ? "PAID" : "FAILED",
                    transactionId = transactionId,
                    orderCode = orderCode,
                    amount = amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay callback: {@Request}", request);
                return StatusCode(500, new
                {
                    success = false,
                    error = $"Error processing payment: {ex.Message}"
                });
            }
        }
        /// <summary>
        /// Nạp tiền vào ví shop
        /// </summary>
        [HttpPost("deposit")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DepositResponseDto>> CreateDeposit([FromBody] DepositRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Lấy thông tin user từ JWT
                var userId = _currentUserService.GetUserId().ToString();
                var shopIdClaim = User.FindFirst("ShopId")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { error = "Không tìm thấy thông tin người dùng" });

                // Xác định shopId - ưu tiên từ request, nếu không có thì lấy từ JWT
                Guid shopId;
                if (request.ShopId.HasValue)
                {
                    shopId = request.ShopId.Value;
                }
                else if (!string.IsNullOrEmpty(shopIdClaim) && Guid.TryParse(shopIdClaim, out var parsedShopId))
                {
                    shopId = parsedShopId;
                }
                else
                {
                    return BadRequest(new { error = "Không tìm thấy thông tin shop" });
                }

                // Kiểm tra shop có tồn tại không
                var shopExists = await _walletServiceClient.DoesShopExistAsync(shopId);
                if (!shopExists)
                    return BadRequest(new { error = "Shop không tồn tại hoặc không có ví" });

                // Tạo mã QR code cho deposit
                var qrCode = await _qrCodeService.GenerateDepositQrCodeAsync(
                    shopId, 
                    request.Amount,
                    Guid.Parse(userId),
                    PaymentMethod.BankTransfer
                );

                var createPaymentDto = new CreatePaymentDto
                {
                    OrderId = shopId, 
                    Amount = request.Amount,
                    PaymentMethod = PaymentMethod.BankTransfer,
                    CreatedBy = userId,
                    QrCode = qrCode
                };

                var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);

                var response = new DepositResponseDto
                {
                    QrCode = qrCode,
                    PaymentId = payment.Id,
                    Amount = request.Amount,
                    ShopId = shopId,
                    Description = request.Description ?? $"Nạp {request.Amount:N0}đ vào ví shop",
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Tạo deposit thành công cho shop {ShopId}, số tiền {Amount}",
                    shopId, request.Amount);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo deposit cho shop");
                return StatusCode(500, new { error = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpPost("callback/deposit")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessDepositCallback([FromBody] SePayCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received deposit callback: {@Request}", request);

                var transactionId = request.TransactionId;
                var orderCode = request.OrderCode;
                var amount = request.Amount;
                var status = request.Status;

                _logger.LogInformation("Deposit callback - TransactionId: {TransactionId}, OrderCode: {OrderCode}, Amount: {Amount}, Status: {Status}",
                    transactionId, orderCode, amount, status);

                if (string.IsNullOrEmpty(orderCode))
                {
                    _logger.LogWarning("Order code rỗng trong deposit callback");
                    return BadRequest(new { success = false, error = "Order code không được để trống" });
                }

                // Extract shopId from orderCode for deposit
                Guid shopId;
                try
                {
                    if (orderCode.StartsWith("DEPOSIT_", StringComparison.OrdinalIgnoreCase))
                    {
                        var shopIdString = orderCode.Substring(8); // Remove "DEPOSIT_"
                        if (shopIdString.Length == 32) // GUID without hyphens
                        {
                            var formattedGuid = $"{shopIdString.Substring(0, 8)}-{shopIdString.Substring(8, 4)}-{shopIdString.Substring(12, 4)}-{shopIdString.Substring(16, 4)}-{shopIdString.Substring(20, 12)}";
                            shopId = Guid.Parse(formattedGuid);
                        }
                        else
                        {
                            shopId = Guid.Parse(shopIdString); // GUID with hyphens
                        }
                    }
                    else if (orderCode.Length == 32) // Direct GUID without prefix
                    {
                        // Format GUID từ 32 ký tự
                        var formattedGuid = $"{orderCode.Substring(0, 8)}-{orderCode.Substring(8, 4)}-{orderCode.Substring(12, 4)}-{orderCode.Substring(16, 4)}-{orderCode.Substring(20, 12)}";
                        shopId = Guid.Parse(formattedGuid);
                    }
                    else
                    {
                        shopId = Guid.Parse(orderCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi parse shopId từ orderCode: {OrderCode}", orderCode);
                    return BadRequest(new { success = false, error = $"OrderCode không hợp lệ: {orderCode}" });
                }

                // Tìm payment bằng shopId (đã dùng làm orderId)
                var payments = await _paymentService.GetPaymentsByOrderIdAsync(shopId);
                var payment = payments?.FirstOrDefault();

                if (payment == null)
                {
                    _logger.LogWarning("Không tìm thấy payment cho deposit của shop: {ShopId}", shopId);
                    return BadRequest(new { success = false, error = "Không tìm thấy giao dịch payment" });
                }

                // Xử lý callback payment
                var callbackDto = new PaymentCallbackDto
                {
                    IsSuccessful = status == "success",
                    QrCode = payment.QrCode,
                    RawResponse = System.Text.Json.JsonSerializer.Serialize(request)
                };

                var result = await _paymentService.ProcessPaymentCallbackAsync(payment.Id, callbackDto);

                // Nếu payment thành công, tạo wallet transaction
                if (status == "success")
                {
                    var walletTransactionRequest = new CreateWalletTransactionRequest
                    {
                        Type = 1, // Deposit = 1 (dựa vào enum WalletTransactionType)
                        Amount = amount,
                        ShopId = shopId,
                        Status = 0, // Success = 0
                        TransactionId = transactionId,
                        Description = $"Nạp {amount:N0}đ vào ví shop",
                        CreatedBy = "System"
                    };

                    var walletResult = await _walletServiceClient.CreateWalletTransactionAsync(walletTransactionRequest);
                    if (!walletResult)
                    {
                        _logger.LogWarning("Tạo wallet transaction thất bại cho deposit shopId: {ShopId}", shopId);
                    }
                    else
                    {
                        _logger.LogInformation("Tạo wallet transaction thành công cho deposit shopId: {ShopId}", shopId);
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = "Xử lý deposit callback thành công",
                    paymentId = payment.Id,
                    shopId = shopId,
                    status = status == "success" ? "SUCCESS" : "FAILED",
                    transactionId = transactionId,
                    amount = amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý deposit callback: {@Request}", request);
                return StatusCode(500, new
                {
                    success = false,
                    error = $"Lỗi xử lý callback: {ex.Message}"
                });
            }
        }
        /// <summary>
        /// Phê duyệt yêu cầu rút tiền và tạo QR code chuyển tiền
        /// </summary>
        [HttpPost("withdrawal-approval")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WithdrawalApprovalResponseDto>> CreateWithdrawalApproval([FromBody] WithdrawalApprovalRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Lấy thông tin user từ JWT
                var userId = _currentUserService.GetUserId();

                _logger.LogInformation("Processing withdrawal approval for transaction {TransactionId} by user {UserId}",
                    request.WalletTransactionId, userId);

                // 1. Lấy thông tin wallet transaction
                var walletTransaction = await _walletServiceClient.GetWalletTransactionByIdAsync(request.WalletTransactionId);
                if (walletTransaction == null)
                {
                    _logger.LogWarning("Wallet transaction not found: {TransactionId}", request.WalletTransactionId);
                    return NotFound(new { error = "Không tìm thấy giao dịch rút tiền" });
                }

                // 2. Kiểm tra trạng thái giao dịch
                if (walletTransaction.Status != "Pending")
                {
                    _logger.LogWarning("Invalid transaction status for approval: {Status}", walletTransaction.Status);
                    return BadRequest(new { error = "Giao dịch không ở trạng thái chờ phê duyệt" });
                }

                // 3. Kiểm tra loại giao dịch
                if (walletTransaction.Type != "Withdraw")
                {
                    _logger.LogWarning("Invalid transaction type for withdrawal: {Type}", walletTransaction.Type);
                    return BadRequest(new { error = "Đây không phải giao dịch rút tiền" });
                }

                // 4. Tạo QR code cho việc chuyển tiền về shop
                var qrCode = await _qrCodeService.GenerateWithdrawalQrCodeAsync(
                    walletTransaction.Id,
                    Math.Abs(walletTransaction.Amount), // Amount rút tiền là số âm, cần chuyển thành dương
                    userId,
                    PaymentMethod.BankTransfer,
                    walletTransaction.BankAccount,
                    walletTransaction.BankNumber
                );

                // 5. Tạo payment record để track việc chuyển tiền
                var createPaymentDto = new CreatePaymentDto
                {
                    OrderId = walletTransaction.Id, // Sử dụng walletTransactionId làm orderId
                    Amount = Math.Abs(walletTransaction.Amount),
                    PaymentMethod = PaymentMethod.BankTransfer,
                    CreatedBy = userId.ToString(),
                    QrCode = qrCode
                };

                var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);

                var response = new WithdrawalApprovalResponseDto
                {
                    PaymentId = payment.Id,
                    WalletTransactionId = walletTransaction.Id,
                    QrCode = qrCode,
                    Amount = Math.Abs(walletTransaction.Amount),
                    BankAccount = walletTransaction.BankAccount,
                    BankNumber = walletTransaction.BankNumber,
                    Description = $"Chuyển {Math.Abs(walletTransaction.Amount):N0}đ về {walletTransaction.BankAccount} - {walletTransaction.BankNumber}",
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Created withdrawal approval for transaction {TransactionId}, payment {PaymentId}",
                    request.WalletTransactionId, payment.Id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating withdrawal approval for transaction {TransactionId}",
                    request.WalletTransactionId);
                return StatusCode(500, new { error = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        /// <summary>
        /// Callback xử lý kết quả chuyển tiền rút về
        /// </summary>
        [HttpPost("callback/withdrawal")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessWithdrawalCallback([FromBody] SePayCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received withdrawal callback: {@Request}", request);

                var transactionId = request.TransactionId;
                var orderCode = request.OrderCode;
                var amount = request.Amount;
                var status = request.Status;

                _logger.LogInformation("Withdrawal callback - TransactionId: {TransactionId}, OrderCode: {OrderCode}, Amount: {Amount}, Status: {Status}",
                    transactionId, orderCode, amount, status);

                if (string.IsNullOrEmpty(orderCode))
                {
                    _logger.LogWarning("Order code rỗng trong withdrawal callback");
                    return BadRequest(new { success = false, error = "Order code không được để trống" });
                }

                // Extract walletTransactionId from orderCode
                Guid walletTransactionId;
                try
                {
                    if (orderCode.StartsWith("WITHDRAW_", StringComparison.OrdinalIgnoreCase))
                    {
                        var transactionIdString = orderCode.Substring(9); 
                        if (transactionIdString.Length == 32) 
                        {
                            var formattedGuid = $"{transactionIdString.Substring(0, 8)}-{transactionIdString.Substring(8, 4)}-{transactionIdString.Substring(12, 4)}-{transactionIdString.Substring(16, 4)}-{transactionIdString.Substring(20, 12)}";
                            walletTransactionId = Guid.Parse(formattedGuid);
                        }
                        else
                        {
                            walletTransactionId = Guid.Parse(transactionIdString); 
                        }
                    }
                    else if (orderCode.Length == 32) 
                    {
                        var formattedGuid = $"{orderCode.Substring(0, 8)}-{orderCode.Substring(8, 4)}-{orderCode.Substring(12, 4)}-{orderCode.Substring(16, 4)}-{orderCode.Substring(20, 12)}";
                        walletTransactionId = Guid.Parse(formattedGuid);
                    }
                    else
                    {
                        walletTransactionId = Guid.Parse(orderCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi parse walletTransactionId từ orderCode: {OrderCode}", orderCode);
                    return BadRequest(new { success = false, error = $"OrderCode không hợp lệ: {orderCode}" });
                }

                // Tìm payment bằng walletTransactionId (đã dùng làm orderId)
                var payments = await _paymentService.GetPaymentsByOrderIdAsync(walletTransactionId);
                var payment = payments?.FirstOrDefault();

                if (payment == null)
                {
                    _logger.LogWarning("Không tìm thấy payment cho withdrawal transaction: {TransactionId}", walletTransactionId);
                    return BadRequest(new { success = false, error = "Không tìm thấy giao dịch payment" });
                }

                // Xử lý callback payment
                var callbackDto = new PaymentCallbackDto
                {
                    IsSuccessful = status == "success",
                    QrCode = payment.QrCode,
                    RawResponse = System.Text.Json.JsonSerializer.Serialize(request)
                };

                var result = await _paymentService.ProcessPaymentCallbackAsync(payment.Id, callbackDto);

                // 2. Cập nhật trạng thái wallet transaction
                var walletTransactionStatus = status == "success" ? 0 : 1; // Success = 0, Failed = 1

                var updateResult = await _walletServiceClient.UpdateWalletTransactionStatusAsync(
                    walletTransactionId,
                    walletTransactionStatus,
                    transactionId,
                    "System"
                );

                if (updateResult)
                {
                    _logger.LogInformation("Cập nhật wallet transaction status thành công: {TransactionId}", walletTransactionId);
                }
                else
                {
                    _logger.LogWarning("Cập nhật wallet transaction status thất bại: {TransactionId}", walletTransactionId);
                }

                return Ok(new
                {
                    success = true,
                    message = "Xử lý withdrawal callback thành công",
                    paymentId = payment.Id,
                    walletTransactionId = walletTransactionId,
                    status = status == "success" ? "SUCCESS" : "FAILED",
                    transactionId = transactionId,
                    amount = amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý withdrawal callback: {@Request}", request);
                return StatusCode(500, new
                {
                    success = false,
                    error = $"Lỗi xử lý callback: {ex.Message}"
                });
            }
        }


        ///////Xử lý lạc việc call back nhiều lần 
        /// <summary>
        /// 🎯 UNIFIED CALLBACK - Xử lý TẤT CẢ callback từ SePay qua 1 endpoint duy nhất
        /// Supports: Orders, Deposits, Withdrawals với cả redirect và JSON response
        /// </summary>
        [HttpPost("callback/unified")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessUnifiedSePayCallback([FromBody] SePayCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("🔔 Received unified SePay callback: {@Request}", request);

                // ✅ EXTRACT BASIC INFO
                var transactionId = request.TransactionId;
                var orderCode = request.OrderCode;
                var amount = request.Amount;
                var status = request.Status;
                var transferType = request.TransferType ?? "in"; // Default to "in" if not provided

                _logger.LogInformation("📊 Processing - TransactionId: {TransactionId}, OrderCode: {OrderCode}, Amount: {Amount}, Status: {Status}, TransferType: {TransferType}",
                    transactionId, orderCode, amount, status, transferType);

                if (string.IsNullOrEmpty(orderCode))
                {
                    _logger.LogWarning("⚠️ Order code empty in callback");
                    return BadRequest(new { success = false, error = "Order code không được để trống" });
                }

                // ✅ DETERMINE CALLBACK TYPE BASED ON ORDERCODE AND TRANSFERTYPE
                var callbackType = DetermineCallbackTypeEnhanced(orderCode, transferType, request.Content);
                _logger.LogInformation("🎯 Detected callback type: {CallbackType}", callbackType);

                return callbackType switch
                {
                    CallbackType.Order => await ProcessOrderCallback(request),
                    CallbackType.Deposit => await ProcessDepositCallbackInternal(request),
                    CallbackType.Withdrawal => await ProcessWithdrawalCallbackInternal(request),
                    _ => BadRequest(new { success = false, error = $"Không hỗ trợ loại callback: {orderCode}" })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error processing unified SePay callback");
                return HandleCallbackError(ex);
            }
        }

        #region Private Helper Methods

        ///// <summary>
        ///// Determines the type of callback based on order code and transfer type
        ///// </summary>
        //private CallbackType DetermineCallbackType(string orderCode, string transferType)
        //{
        //    // For money OUT (withdrawals)
        //    if (transferType == "out")
        //    {
        //        if (orderCode.StartsWith("WITHDRAW_", StringComparison.OrdinalIgnoreCase) ||
        //            orderCode.StartsWith("WITHDRAW_CONFIRM_", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return CallbackType.Withdrawal;
        //        }
        //        // Manual withdrawal without proper prefix - check content for GUID pattern
        //        if (Guid.TryParse(ExtractGuidFromString(orderCode), out _))
        //        {
        //            return CallbackType.Withdrawal;
        //        }
        //    }

        //    // For money IN (deposits and orders)
        //    if (orderCode.StartsWith("DEPOSIT_", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return CallbackType.Deposit;
        //    }

        //    if (orderCode.StartsWith("ORDER", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return CallbackType.Order;
        //    }

        //    // Default fallback based on transferType
        //    return transferType == "out" ? CallbackType.Withdrawal : CallbackType.Order;
        //}
        private CallbackType DetermineCallbackTypeEnhanced(string orderCode, string transferType, string? content)
        {
            _logger.LogInformation("🔍 Analyzing callback - OrderCode: {OrderCode}, TransferType: {TransferType}, Content: {Content}",
                orderCode, transferType, content);

            // For money OUT (withdrawals)
            if (transferType == "out")
            {
                if (orderCode.StartsWith("WITHDRAW_", StringComparison.OrdinalIgnoreCase) ||
                    orderCode.StartsWith("WITHDRAW_CONFIRM_", StringComparison.OrdinalIgnoreCase))
                {
                    return CallbackType.Withdrawal;
                }

                // Check content for withdrawal patterns
                if (!string.IsNullOrEmpty(content) &&
                    (content.Contains("WITHDRAW", StringComparison.OrdinalIgnoreCase) ||
                     Regex.IsMatch(content, @"[0-9a-fA-F]{32}", RegexOptions.IgnoreCase)))
                {
                    return CallbackType.Withdrawal;
                }
            }
            if (transferType == "in")
            {
                if (orderCode.StartsWith("DEPOSIT_", StringComparison.OrdinalIgnoreCase))
                {
                    return CallbackType.Deposit;
                }

                if (orderCode.StartsWith("ORDER", StringComparison.OrdinalIgnoreCase))
                {
                    return CallbackType.Order;
                }

                // ✅ Additional check for content-based detection
                if (!string.IsNullOrEmpty(content))
                {
                    if (content.Contains("DEPOSIT", StringComparison.OrdinalIgnoreCase))
                    {
                        return CallbackType.Deposit;
                    }

                    if (content.Contains("ORDER", StringComparison.OrdinalIgnoreCase))
                    {
                        return CallbackType.Order;
                    }
                }
            }

            // Fallback based on transferType
            return transferType == "out" ? CallbackType.Withdrawal : CallbackType.Order;
        }
        /// <summary>
        /// Processes order payment callbacks (single and bulk orders)
        /// </summary>
        private async Task<IActionResult> ProcessOrderCallback(SePayCallbackRequest request)
        {
            var orderIds = ExtractOrderIds(request.OrderCode!);
            if (!orderIds.Any())
            {
                return BadRequest(new { success = false, error = "No valid order IDs found" });
            }

            // Find payment by first order ID
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderIds.First());
            var payment = payments?.FirstOrDefault();

            if (payment?.QrCode == null)
            {
                return BadRequest(new { success = false, error = "Payment hoặc QR Code không tìm thấy" });
            }

            // Process payment callback
            var callbackDto = new PaymentCallbackDto
            {
                IsSuccessful = request.Status == "success",
                QrCode = payment.QrCode,
                RawResponse = System.Text.Json.JsonSerializer.Serialize(request)
            };

            await _paymentService.ProcessPaymentCallbackAsync(payment.Id, callbackDto);

            // Determine if should redirect or return JSON
            if (ShouldRedirect(request))
            {
                return BuildOrderRedirectResponse(request.Status!, orderIds);
            }

            return Ok(new
            {
                success = true,
                type = "ORDER",
                message = "Order payment processed successfully",
                paymentId = payment.Id,
                orderIds = orderIds,
                status = request.Status == "success" ? "PAID" : "FAILED",
                transactionId = request.TransactionId,
                amount = request.Amount
            });
        }

        /// <summary>
        /// Processes deposit callbacks
        /// </summary>
        private async Task<IActionResult> ProcessDepositCallbackInternal(SePayCallbackRequest request)
        {
            try
            {
                var shopId = ExtractShopIdFromOrderCodeEnhanced(request.OrderCode!, request.Content);
                if (shopId == Guid.Empty)
                {
                    _logger.LogWarning("❌ Could not extract valid ShopId from OrderCode: {OrderCode}, Content: {Content}",
                        request.OrderCode, request.Content);

                    return BadRequest(new
                    {
                        success = false,
                        error = "ShopId không hợp lệ",
                        orderCode = request.OrderCode,
                        content = request.Content,
                        extractedShopId = shopId
                    });
                }

                _logger.LogInformation("✅ Extracted ShopId: {ShopId} from OrderCode: {OrderCode}", shopId, request.OrderCode);

                var payments = await _paymentService.GetPaymentsByOrderIdAsync(shopId);
                var payment = payments?.FirstOrDefault();

                if (payment == null)
                {
                    _logger.LogWarning("❌ Payment not found for deposit ShopId: {ShopId}", shopId);
                    return BadRequest(new
                    {
                        success = false,
                        error = "Không tìm thấy giao dịch payment",
                        shopId = shopId,
                        orderCode = request.OrderCode
                    });
                }

                // Process payment callback
                var callbackDto = new PaymentCallbackDto
                {
                    IsSuccessful = request.Status == "success",
                    QrCode = payment.QrCode,
                    RawResponse = System.Text.Json.JsonSerializer.Serialize(request)
                };

                await _paymentService.ProcessPaymentCallbackAsync(payment.Id, callbackDto);

                // Create wallet transaction if successful
                if (request.Status == "success")
                {
                    await CreateDepositWalletTransaction(shopId, request.Amount, request.TransactionId!);
                }

                return Ok(new
                {
                    success = true,
                    type = "DEPOSIT",
                    message = "Xử lý deposit callback thành công",
                    paymentId = payment.Id,
                    shopId = shopId,
                    status = request.Status == "success" ? "SUCCESS" : "FAILED",
                    transactionId = request.TransactionId,
                    amount = request.Amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error processing deposit callback");
                throw;
            }
        }
        private Guid ExtractShopIdFromOrderCodeEnhanced(string orderCode, string? content)
        {
            try
            {
                _logger.LogInformation("🔍 Extracting ShopId from OrderCode: {OrderCode}, Content: {Content}", orderCode, content);

                // Primary extraction from orderCode
                if (orderCode.StartsWith("DEPOSIT_", StringComparison.OrdinalIgnoreCase))
                {
                    var shopIdString = orderCode.Substring(8);
                    var shopId = ParseGuidFromString(shopIdString);
                    if (shopId != Guid.Empty)
                    {
                        _logger.LogInformation("✅ Extracted ShopId from DEPOSIT_ prefix: {ShopId}", shopId);
                        return shopId;
                    }
                }

                // Fallback: Extract from orderCode directly if it's a GUID
                if (orderCode.Length == 32 || orderCode.Length == 36)
                {
                    var shopId = ParseGuidFromString(orderCode);
                    if (shopId != Guid.Empty)
                    {
                        _logger.LogInformation("✅ Extracted ShopId from direct GUID: {ShopId}", shopId);
                        return shopId;
                    }
                }

                // ✅ NEW: Fallback extraction from content
                if (!string.IsNullOrEmpty(content))
                {
                    var guidPattern = @"[0-9a-fA-F]{32}";
                    var matches = Regex.Matches(content, guidPattern, RegexOptions.IgnoreCase);

                    foreach (Match match in matches)
                    {
                        var shopId = ParseGuidFromString(match.Value);
                        if (shopId != Guid.Empty)
                        {
                            _logger.LogInformation("✅ Extracted ShopId from content pattern: {ShopId}", shopId);
                            return shopId;
                        }
                    }
                }

                _logger.LogWarning("❌ Could not extract valid ShopId from any source");
                return Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error extracting ShopId from: {OrderCode}", orderCode);
                return Guid.Empty;
            }
        }
        /// <summary>
        /// Processes withdrawal callbacks
        /// </summary>
        private async Task<IActionResult> ProcessWithdrawalCallbackInternal(SePayCallbackRequest request)
        {
            var walletTransactionId = ExtractWalletTransactionId(request.OrderCode!);
            if (walletTransactionId == Guid.Empty)
            {
                return BadRequest(new { success = false, error = "WalletTransactionId không hợp lệ" });
            }

            var payments = await _paymentService.GetPaymentsByOrderIdAsync(walletTransactionId);
            var payment = payments?.FirstOrDefault();

            if (payment == null)
            {
                return BadRequest(new { success = false, error = "Không tìm thấy giao dịch payment" });
            }

            // Process payment callback
            var callbackDto = new PaymentCallbackDto
            {
                IsSuccessful = request.Status == "success",
                QrCode = payment.QrCode,
                RawResponse = System.Text.Json.JsonSerializer.Serialize(request)
            };

            await _paymentService.ProcessPaymentCallbackAsync(payment.Id, callbackDto);

            // Update wallet transaction status
            var walletTransactionStatus = request.Status == "success" ? 0 : 1;
            var updateResult = await _walletServiceClient.UpdateWalletTransactionStatusAsync(
                walletTransactionId,
                walletTransactionStatus,
                request.TransactionId!,
                "System"
            );

            _logger.LogInformation("Wallet transaction status update result: {Result} for {TransactionId}",
                updateResult, walletTransactionId);

            return Ok(new
            {
                success = true,
                type = "WITHDRAWAL",
                message = "Xử lý withdrawal callback thành công",
                paymentId = payment.Id,
                walletTransactionId = walletTransactionId,
                status = request.Status == "success" ? "SUCCESS" : "FAILED",
                transactionId = request.TransactionId,
                amount = request.Amount
            });
        }

        /// <summary>
        /// Extracts order IDs from order code (handles single and bulk)
        /// </summary>
        private List<Guid> ExtractOrderIds(string orderCode)
        {
            var orderIds = new List<Guid>();

            try
            {
                if (orderCode.StartsWith("ORDERS_", StringComparison.OrdinalIgnoreCase))
                {
                    var orderIdsString = orderCode.Substring(7);
                    orderIds = orderIdsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => Guid.Parse(id.Trim()))
                        .ToList();
                }
                else if (orderCode.StartsWith("ORDER_", StringComparison.OrdinalIgnoreCase))
                {
                    var orderIdString = orderCode.Substring(6);
                    orderIds.Add(Guid.Parse(orderIdString));
                }
                else if (orderCode.StartsWith("ORDER", StringComparison.OrdinalIgnoreCase))
                {
                    var guidString = ExtractGuidFromString(orderCode.Substring(5));
                    if (Guid.TryParse(guidString, out var orderId))
                    {
                        orderIds.Add(orderId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting order IDs from: {OrderCode}", orderCode);
            }

            return orderIds;
        }

        /// <summary>
        /// Extracts shop ID from deposit order code
        /// </summary>
        private Guid ExtractShopIdFromOrderCode(string orderCode)
        {
            try
            {
                if (orderCode.StartsWith("DEPOSIT_", StringComparison.OrdinalIgnoreCase))
                {
                    var shopIdString = orderCode.Substring(8);
                    return ParseGuidFromString(shopIdString);
                }
                else if (orderCode.Length == 32 || orderCode.Length == 36)
                {
                    return ParseGuidFromString(orderCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting shop ID from: {OrderCode}", orderCode);
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Extracts wallet transaction ID from withdrawal order code
        /// </summary>
        private Guid ExtractWalletTransactionId(string orderCode)
        {
            try
            {
                if (orderCode.StartsWith("WITHDRAW_CONFIRM_", StringComparison.OrdinalIgnoreCase))
                {
                    var transactionIdString = orderCode.Substring(17);
                    return ParseGuidFromString(transactionIdString);
                }
                else if (orderCode.StartsWith("WITHDRAW_", StringComparison.OrdinalIgnoreCase))
                {
                    var transactionIdString = orderCode.Substring(9);
                    return ParseGuidFromString(transactionIdString);
                }
                else if (orderCode.Length == 32 || orderCode.Length == 36)
                {
                    return ParseGuidFromString(orderCode);
                }
                else
                {
                    // Try to extract GUID from content for manual transfers
                    var guidString = ExtractGuidFromString(orderCode);
                    return ParseGuidFromString(guidString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting wallet transaction ID from: {OrderCode}", orderCode);
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Extracts GUID pattern from string using regex
        /// </summary>
        private string ExtractGuidFromString(string input)
        {
            var guidPattern = @"[0-9a-fA-F]{32}";
            var match = System.Text.RegularExpressions.Regex.Match(input, guidPattern);
            return match.Success ? match.Value : input;
        }

        /// <summary>
        /// Parses GUID from string (handles both 32-char and hyphenated formats)
        /// </summary>
        private Guid ParseGuidFromString(string guidString)
        {
            if (guidString.Length == 32)
            {
                var formattedGuid = $"{guidString.Substring(0, 8)}-{guidString.Substring(8, 4)}-{guidString.Substring(12, 4)}-{guidString.Substring(16, 4)}-{guidString.Substring(20, 12)}";
                return Guid.Parse(formattedGuid);
            }
            return Guid.Parse(guidString);
        }

        /// <summary>
        /// Determines if should redirect based on request characteristics
        /// </summary>
        private bool ShouldRedirect(SePayCallbackRequest request)
        {
            // Check User-Agent for browser requests
            var userAgent = Request.Headers["User-Agent"].ToString();
            return !string.IsNullOrEmpty(userAgent) &&
                   (userAgent.Contains("Mozilla") || userAgent.Contains("Chrome") || userAgent.Contains("Safari"));
        }

        /// <summary>
        /// Builds redirect response for order payments
        /// </summary>
        private IActionResult BuildOrderRedirectResponse(string status, List<Guid> orderIds)
        {
            var baseRedirectUrl = status == "success"
                ? _configuration["PaymentRedirects:SuccessUrl"]
                : _configuration["PaymentRedirects:FailureUrl"];

            if (string.IsNullOrEmpty(baseRedirectUrl))
            {
                baseRedirectUrl = status == "success"
                    ? "https://stream-cart-fe.vercel.app/payment/order/results-success"
                    : "https://stream-cart-fe.vercel.app/payment/order/results-failed";
            }

            string redirectUrl;
            if (orderIds.Count == 1)
            {
                redirectUrl = $"{baseRedirectUrl.TrimEnd('/')}?orders={orderIds.First()}";
            }
            else
            {
                var orderIdsString = string.Join(",", orderIds);
                redirectUrl = $"{baseRedirectUrl.TrimEnd('/')}?orders={orderIdsString}";
            }

            _logger.LogInformation("Redirecting to: {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Creates deposit wallet transaction
        /// </summary>
        private async Task CreateDepositWalletTransaction(Guid shopId, decimal amount, string transactionId)
        {
            try
            {
                var walletTransactionRequest = new CreateWalletTransactionRequest
                {
                    Type = 1, // Deposit = 1
                    Amount = amount,
                    ShopId = shopId,
                    Status = 0, // Success = 0
                    TransactionId = transactionId,
                    Description = $"Nạp {amount:N0}đ vào ví shop",
                    CreatedBy = "System"
                };

                var result = await _walletServiceClient.CreateWalletTransactionAsync(walletTransactionRequest);
                _logger.LogInformation("Wallet transaction created: {Result} for shop {ShopId}", result, shopId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wallet transaction for shop {ShopId}", shopId);
            }
        }

        /// <summary>
        /// Handles callback errors consistently
        /// </summary>
        private IActionResult HandleCallbackError(Exception ex)
        {
            var errorRedirectUrl = _configuration["PaymentRedirects:FailureUrl"] ??
                "https://stream-cart-fe.vercel.app/payment/order/results-failed";

            // For browser requests, redirect to error page
            if (ShouldRedirect(new SePayCallbackRequest()))
            {
                return Redirect($"{errorRedirectUrl.TrimEnd('/')}/error");
            }

            // For API requests, return JSON error
            return StatusCode(500, new
            {
                success = false,
                error = $"Lỗi xử lý callback: {ex.Message}"
            });
        }

        #endregion

        /// <summary>
        /// Enum for different callback types
        /// </summary>
        private enum CallbackType
        {
            Order,
            Deposit,
            Withdrawal
        }
    }
}