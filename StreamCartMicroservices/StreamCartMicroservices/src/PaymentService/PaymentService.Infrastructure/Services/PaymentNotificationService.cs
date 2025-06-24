using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Services
{
    public class PaymentNotificationService : IPaymentNotificationService
    {
        //private readonly IEmailService _emailService;
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly ILogger<PaymentNotificationService> _logger;

        public PaymentNotificationService(
            //IEmailService emailService,
            IOrderServiceClient orderServiceClient,
            ILogger<PaymentNotificationService> logger)
        {
            //_emailService = emailService;
            _orderServiceClient = orderServiceClient;
            _logger = logger;
        }

        public async Task SendPaymentConfirmationAsync(PaymentDto payment)
        {
            try
            {
                // Implementation will depend on your email service
                // This is a placeholder for the actual implementation
                _logger.LogInformation("Sending payment confirmation for Payment {PaymentId}, Order {OrderId}",
                    payment.Id, payment.OrderId);

                // Update order status
                await _orderServiceClient.UpdateOrderPaymentStatusAsync(payment.OrderId, "Paid");

                // Could send an email to the user here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment confirmation for Payment {PaymentId}", payment.Id);
            }
        }

        public async Task SendPaymentFailureAsync(PaymentDto payment, string reason)
        {
            try
            {
                _logger.LogInformation("Sending payment failure notification for Payment {PaymentId}, Order {OrderId}: {Reason}",
                    payment.Id, payment.OrderId, reason);

                // Update order status
                await _orderServiceClient.UpdateOrderPaymentStatusAsync(payment.OrderId, "PaymentFailed");

                // Could send an email to the user here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment failure notification for Payment {PaymentId}", payment.Id);
            }
        }

        public async Task SendRefundNotificationAsync(PaymentDto payment)
        {
            try
            {
                _logger.LogInformation("Sending refund notification for Payment {PaymentId}, Order {OrderId}",
                    payment.Id, payment.OrderId);

                // Update order status
                await _orderServiceClient.UpdateOrderPaymentStatusAsync(payment.OrderId, "Refunded");

                // Could send an email to the user here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending refund notification for Payment {PaymentId}", payment.Id);
            }
        }
    }
}