using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Wallet
{
    public class ShopPaymentDTO
    {
        public Guid OrderId { get; set; }
        public Guid ShopId { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public string? TransactionType { get; set; }
        public string? TransactionReference { get; set; }
        public string? Description { get; set; }
    }
}
