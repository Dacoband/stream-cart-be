using MassTransit;
using Microsoft.Extensions.Logging;
using OrderService.Application.Events;
using OrderService.Application.Interfaces;
using OrderService.Application.Interfaces.IRepositories;
using Shared.Messaging.Consumers;
using System;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Messaging.Consumers
{
    /// <summary>
    /// Consumer for the AccountUpdated event
    /// </summary>
    public class AccountUpdatedConsumer : IConsumer<AccountUpdated>, IBaseConsumer
    {
        private readonly ILogger<AccountUpdatedConsumer> _logger;
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// Creates a new instance of AccountUpdatedConsumer
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="orderRepository">Order repository</param>
        public AccountUpdatedConsumer(
            ILogger<AccountUpdatedConsumer> logger,
            IOrderRepository orderRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        /// <summary>
        /// Consumes the AccountUpdated event
        /// </summary>
        /// <param name="context">Consumer context</param>
        public async Task Consume(ConsumeContext<AccountUpdated> context)
        {
            var message = context.Message;
            
            _logger.LogInformation(
                "Account updated: {AccountId}, Email: {Email}, FullName: {FullName}",
                message.AccountId,
                message.Email,
                message.FullName);

            try
            {
                // This is a placeholder for how you might update local account data
                // in OrderService based on updates from AccountService
                
                _logger.LogInformation("Successfully processed AccountUpdated event for {AccountId}", message.AccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AccountUpdated event for AccountId: {AccountId}", message.AccountId);
            }
        }
    }
}