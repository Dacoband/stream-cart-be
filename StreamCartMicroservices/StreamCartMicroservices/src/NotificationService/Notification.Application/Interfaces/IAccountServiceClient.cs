using Notification.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.Interfaces
{
    public interface IAccountServiceClient
    {
        Task<IEnumerable<ShopAccountDto?>> GetAccountByShopIdAsync(Guid shopId);
        Task<AccountDto> GetAccountByIdAsync(Guid accountId);
    }
}
