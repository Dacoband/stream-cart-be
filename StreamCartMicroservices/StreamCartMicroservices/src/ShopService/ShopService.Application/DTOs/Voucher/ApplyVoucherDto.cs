using System;
using System.ComponentModel.DataAnnotations;

namespace ShopService.Application.DTOs.Voucher
{
    public class ApplyVoucherDto
    {
        [Required(ErrorMessage = "Mã voucher không được để trống")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá trị đơn hàng không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị đơn hàng phải lớn hơn 0")]
        public decimal OrderAmount { get; set; }

        public Guid? OrderId { get; set; }
    }
}