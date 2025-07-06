using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Wallet
{
    public class UpdateWalletDTO
    {
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
    }
}
