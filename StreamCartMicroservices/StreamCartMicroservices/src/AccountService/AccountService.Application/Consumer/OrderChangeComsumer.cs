using AccountService.Infrastructure.Interfaces;
using MassTransit;

using Shared.Messaging.Consumers;
using Shared.Messaging.Event.OrderEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Consumer
{
    public class OrderChangeComsumer : IConsumer<OrderCreatedOrUpdatedEvent>, IBaseConsumer
    {
        private readonly IAccountRepository _accountRepository;
        public OrderChangeComsumer(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        public async Task Consume(ConsumeContext<OrderCreatedOrUpdatedEvent> context)
        {
          if(context.Message.UserRate != 0)
            {
                var existingAccount = await _accountRepository.GetByIdAsync(context.Message.CreatedBy);
                existingAccount.UpdateCompleteRate((decimal)context.Message.UserRate);
                await _accountRepository.ReplaceAsync(context.Message.CreatedBy, existingAccount);
            }  
        }
    }
}
