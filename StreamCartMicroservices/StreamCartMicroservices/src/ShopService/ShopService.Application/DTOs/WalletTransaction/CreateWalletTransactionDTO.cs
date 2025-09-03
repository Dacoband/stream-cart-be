using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.WalletTransaction
{
    public class CreateWalletTransactionDTO
    {
        public WalletTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public WalletTransactionStatus? Status { get; set; } = WalletTransactionStatus.Pending;
        public string? TransactionId { get; set; }
        public Guid? ShopMembershipId { get; set; }
        public string? OrderId { get; set; }
        public Guid? RefundId { get; set; }
    }
}
