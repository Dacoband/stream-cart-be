using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Commands;
using PaymentService.Application.DTOs;
using PaymentService.Application.Events;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.Handlers
{
    public class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand, PaymentDto>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<UpdatePaymentStatusCommandHandler> _logger;

        public UpdatePaymentStatusCommandHandler(
            IPaymentRepository paymentRepository,
            IMessagePublisher messagePublisher,
            ILogger<UpdatePaymentStatusCommandHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        public async Task<PaymentDto> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(request.PaymentId.ToString());
                if (payment == null)
                {
                    throw new ArgumentException($"Payment with ID {request.PaymentId} not found");
                }

                // Set the modifier if provided
                if (!string.IsNullOrEmpty(request.UpdatedBy))
                {
                    payment.SetModifier(request.UpdatedBy);
                }

                // Update status based on the requested new status
                switch (request.NewStatus)
                {
                    case PaymentStatus.Paid:
                        if (string.IsNullOrEmpty(request.QrCode))
                            throw new ArgumentException("QR Code is required for successful payments");

                        payment.MarkAsSuccessful(request.QrCode, request.Fee ?? 0, request.UpdatedBy);
                        break;

                    case PaymentStatus.Failed:
                        payment.MarkAsFailed();
                        break;

                    case PaymentStatus.Refunded:
                        payment.Refund();
                        break;

                    default:
                        throw new ArgumentException($"Status transition to {request.NewStatus} is not supported");
                }

                // Save changes
                await _paymentRepository.ReplaceAsync(payment.Id.ToString(), payment);

                // Publish event if payment is processed (success or failure)
                if (request.NewStatus == PaymentStatus.Paid || request.NewStatus == PaymentStatus.Failed)
                {
                    await _messagePublisher.PublishAsync(new PaymentProcessed
                    {
                        PaymentId = payment.Id,
                        OrderId = payment.OrderId,
                        UserId = payment.UserId,
                        Amount = payment.Amount,
                        Status = payment.Status.ToString(),
                        QrCode = payment.QrCode,
                        ProcessedAt = payment.ProcessedAt ?? DateTime.UtcNow
                    }, cancellationToken);
                }
                else if (request.NewStatus == PaymentStatus.Refunded)
                {
                    await _messagePublisher.PublishAsync(new PaymentRefunded
                    {
                        PaymentId = payment.Id,
                        OrderId = payment.OrderId,
                        UserId = payment.UserId,
                        Amount = payment.Amount,
                        RefundedAt = DateTime.UtcNow
                    }, cancellationToken);
                }

                _logger.LogInformation("Updated payment {PaymentId} status to {Status}", payment.Id, payment.Status);

                // Return updated payment DTO
                return new PaymentDto
                {
                    Id = payment.Id,
                    OrderId = payment.OrderId,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod.ToString(),
                    Status = payment.Status.ToString(),
                    QrCode = payment.QrCode,
                    Fee = payment.Fee,
                    ProcessedAt = payment.ProcessedAt,
                    CreatedAt = payment.CreatedAt,
                    CreatedBy = payment.CreatedBy,
                    LastModifiedAt = payment.LastModifiedAt,
                    LastModifiedBy = payment.LastModifiedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for payment {PaymentId}", request.PaymentId);
                throw;
            }
        }
    }
}