using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Wallet
{
    public class WalletDTO
    {
        public Guid Id { get; set; }
        public string? OwnerType { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public Guid? ShopId { get; set; }
    }
}
