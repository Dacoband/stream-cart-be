using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Application.Interfaces
{
    public interface IAccountServiceClient
    {
        Task<bool> DoesUserExistAsync(Guid userId);
        //Task<string> GetEmailByAccountIdAsync(Guid accountId);
    }
}
