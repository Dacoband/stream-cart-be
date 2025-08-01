using ShopService.Domain.Enums;
using System;

namespace ShopService.Application.DTOs.Voucher
{
    public class VoucherValidationDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public ShopVoucherDto? Voucher { get; set; }
    }
}