using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.WalletTransaction
{
    public class FilterWalletTransactionDTO
    {
        public List<WalletTransactionType>? Types { get; set; }
        public string? Target {  get; set; }
        public string? ShopId { get; set; }
        public List<WalletTransactionStatus>? Status { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public int PageIndex { get; set; } = 1;  
        public int PageSize { get; set; } = 10; 
    }
}
