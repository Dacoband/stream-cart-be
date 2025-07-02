using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public string OwnerType { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public Guid? ShopId { get; set; } 

        public Wallet()
        {
            Balance = 0;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddFunds(decimal amount, string modifiedBy)
        {
            if (amount <= 0)
                throw new ArgumentException("Số tiền phải lớn hơn 0", nameof(amount));

            Balance += amount;
            UpdatedAt = DateTime.UtcNow;
            SetModifier(modifiedBy);
        }

        public void WithdrawFunds(decimal amount, string modifiedBy)
        {
            if (amount <= 0)
                throw new ArgumentException("Số tiền phải lớn hơn 0", nameof(amount));

            if (Balance < amount)
                throw new InvalidOperationException("Số dư không đủ");

            Balance -= amount;
            UpdatedAt = DateTime.UtcNow;
            SetModifier(modifiedBy);
        }
    }
}
