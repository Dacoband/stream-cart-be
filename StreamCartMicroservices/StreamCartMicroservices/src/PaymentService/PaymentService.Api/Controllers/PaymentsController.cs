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

        public PaymentsController(IPaymentService paymentService, IQrCodeService qrCodeService, IOrderServiceClient orderServiceClient)
        {
            _paymentService = paymentService;
            _qrCodeService = qrCodeService;
            _orderServiceClient = orderServiceClient;
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
        public async Task<ActionResult<string>> GenerateQrCode([FromBody] QrCodeRequestDto requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Fix: Pass the required parameters to the GenerateQrCodeAsync method
            var orderDetails = await _orderServiceClient.GetOrderByIdAsync(requestDto.OrderId);
            if (orderDetails == null)
                return NotFound($"Order with ID {requestDto.OrderId} not found");

            var createPaymentDto = new CreatePaymentDto
            {
                OrderId = orderDetails.Id,
                //UserId = orderDetails.UserId,
                Amount = orderDetails.TotalAmount,
                PaymentMethod = PaymentMethod.BankTransfer,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            var payment = await _paymentService.CreatePaymentAsync(createPaymentDto);

            // cập nhật trạng thái đơn hàng 
            await _orderServiceClient.UpdateOrderPaymentStatusAsync(
                orderDetails.Id, PaymentStatus.Pending);


            var qrCode = await _qrCodeService.GenerateQrCodeAsync(
                orderDetails.Id,
                orderDetails.TotalAmount,
                orderDetails.UserId,
                PaymentMethod.BankTransfer 
            );

            // 4. Cập nhật QR code cho Payment nếu cần
                var updateStatusDto = new Application.DTOs.UpdatePaymentStatusDto
                {
                    NewStatus = PaymentStatus.Pending,
                    QrCode = qrCode,
                    UpdatedBy = User.Identity?.Name ?? "System"
                };
                await _paymentService.UpdatePaymentStatusAsync(payment.Id, updateStatusDto);

            return Ok(new
            {
                qrCode = qrCode,
                paymentId = payment.Id,
                amount = orderDetails.TotalAmount
            });
        }
    }
}