using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Domain.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; } 
        public string? Target {  get; set; }
        public string Status { get; set; }
        public string BankAccount { get; set; }
        public string BankNumber { get; set; }
        public string? TransactionId { get; set; }
        public Guid WalletId { get; set; }
        public Guid? ShopMembershipId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? RefundId { get; set; }
    }
}
