using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IAccountCLientService
    {
        Task<AccountDto> GetAccountById(string accountId);
    }
}
