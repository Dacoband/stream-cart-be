using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using Shared.Messaging.Consumers;
using System;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Messaging.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>, IBaseConsumer
    {
        private readonly ILogger<OrderCreatedConsumer> _logger;
        private readonly IPaymentService _paymentService;

        public OrderCreatedConsumer(
            ILogger<OrderCreatedConsumer> logger,
            IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        public async Task Consume(ConsumeContext<OrderCreated> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Order created event received: {OrderId}, Amount: {TotalAmount}, User: {UserId}",
                message.OrderId,
                message.TotalAmount,
                message.UserId);

            try
            {
                // Handle order created event - typically reserving a payment or creating a payment record
                // This would be implemented based on your specific payment flow
                _logger.LogInformation("Payment processing initiated for order {OrderId}", message.OrderId);

                // Example of an asynchronous operation to fix CS1998
                await _paymentService.CreatePaymentAsync(new CreatePaymentDto
                {
                    OrderId = message.OrderId,
                    //UserId = message.UserId,
                    Amount = message.TotalAmount,
                    //CreatedAt = message.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order created event for OrderId: {OrderId}", message.OrderId);
            }
        }
    }

    // DTO for the incoming message from the message broker
    public class OrderCreated
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}