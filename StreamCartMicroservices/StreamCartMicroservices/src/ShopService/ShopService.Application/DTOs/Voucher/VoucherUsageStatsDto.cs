using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Voucher
{
    public class VoucherUsageStatsDto
    {
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public int UsedQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public decimal UsagePercentage { get; set; }
        public DateTime? FirstUsedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public decimal TotalDiscountGiven { get; set; }
        public int UniqueUsersCount { get; set; }
    }
}
