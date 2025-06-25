using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Commands;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;

namespace PaymentService.Application.Handlers
{
    public class ProcessPaymentCallbackCommandHandler : IRequestHandler<ProcessPaymentCallbackCommand, PaymentDto?>
    {
        private readonly IMediator _mediator;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<ProcessPaymentCallbackCommandHandler> _logger;
        private readonly IOrderServiceClient _orderServiceClient;

        public ProcessPaymentCallbackCommandHandler(
            IMediator mediator,
            IPaymentRepository paymentRepository,
            ILogger<ProcessPaymentCallbackCommandHandler> logger,
            IOrderServiceClient orderServiceClient)
        {
            _mediator = mediator;
            _paymentRepository = paymentRepository;
            _logger = logger;
            _orderServiceClient = orderServiceClient;
        }

        public async Task<PaymentDto?> Handle(ProcessPaymentCallbackCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(request.PaymentId.ToString());
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found when processing callback", request.PaymentId);
                    return null;
                }

                var updateCommand = new UpdatePaymentStatusCommand
                {
                    PaymentId = request.PaymentId,
                    NewStatus = request.IsSuccessful ? Domain.Enums.PaymentStatus.Paid : Domain.Enums.PaymentStatus.Failed,
                    QrCode = request.QrCode,
                    Fee = request.Fee,
                    UpdatedBy = "PaymentProcessor"
                };

                var result = await _mediator.Send(updateCommand, cancellationToken);
                if (request.IsSuccessful && result != null)
                {
                    try
                    {
                        // Gọi OrderService để cập nhật trạng thái đơn hàng
                        await _orderServiceClient.UpdateOrderPaymentStatusAsync(
                            payment.OrderId,
                            Domain.Enums.PaymentStatus.Paid);

                        _logger.LogInformation("Order {OrderId} payment status updated to Paid", payment.OrderId);
                    }
                    catch (Exception ex)
                    {
                        // Không ném lỗi ra ngoài vì payment đã được cập nhật thành công
                        _logger.LogError(ex, "Failed to update order status for Order {OrderId}", payment.OrderId);

                        // Có thể thêm cơ chế retry hoặc ghi vào queue để xử lý sau
                    }
                }
                _logger.LogInformation("Payment callback processed for {PaymentId}, status: {IsSuccessful}",
                    request.PaymentId, request.IsSuccessful);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback for {PaymentId}", request.PaymentId);
                throw;
            }
        }
    }
}