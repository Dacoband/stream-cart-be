using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Voucher
{
    public class VoucherApplicationDto
    {
        public bool IsApplied { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public Guid? VoucherId { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public DateTime? AppliedAt { get; set; }
    }
}
