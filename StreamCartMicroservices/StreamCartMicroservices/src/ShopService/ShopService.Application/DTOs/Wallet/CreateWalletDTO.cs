using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Wallet
{
    public class CreateWalletDTO
    {
        public string? OwnerType { get; set; } // "Shop", "User", "System"
        public Guid OwnerId { get; set; } // ID của chủ sở hữu (ShopId hoặc UserId)
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
    }
}
