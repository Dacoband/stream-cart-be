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

        public ProcessPaymentCallbackCommandHandler(
            IMediator mediator,
            IPaymentRepository paymentRepository,
            ILogger<ProcessPaymentCallbackCommandHandler> logger)
        {
            _mediator = mediator;
            _paymentRepository = paymentRepository;
            _logger = logger;
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