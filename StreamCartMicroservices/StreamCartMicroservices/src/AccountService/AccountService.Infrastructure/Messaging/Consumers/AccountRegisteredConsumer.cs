using AccountService.Infrastructure.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Messaging.Consumers
{
    public class AccountRegisteredConsumer : IConsumer<AccountRegistered>
    {
        private readonly ILogger<AccountRegisteredConsumer> _logger;

        public AccountRegisteredConsumer(ILogger<AccountRegisteredConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<AccountRegistered> context)
        {
            _logger.LogInformation(
                "Account registered: {AccountId}, Username: {Username}, Email: {Email}, Role: {Role}",
                context.Message.AccountId,
                context.Message.Username,
                context.Message.Email,
                context.Message.Role);
                
            // Add your business logic here, like sending welcome email
            
            return Task.CompletedTask;
        }
    }
}