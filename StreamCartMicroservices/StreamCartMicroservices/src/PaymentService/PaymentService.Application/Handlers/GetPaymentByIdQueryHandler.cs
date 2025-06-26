using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries;

namespace PaymentService.Application.Handlers
{
    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<GetPaymentByIdQueryHandler> _logger;

        public GetPaymentByIdQueryHandler(
            IPaymentRepository paymentRepository,
            ILogger<GetPaymentByIdQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(request.PaymentId.ToString());
                if (payment == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found", request.PaymentId);
                    return null;
                }

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
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", request.PaymentId);
                throw;
            }
        }
    }
}