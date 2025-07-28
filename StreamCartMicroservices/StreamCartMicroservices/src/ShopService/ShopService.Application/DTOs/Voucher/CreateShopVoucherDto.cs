using ShopService.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ShopService.Application.DTOs.Voucher
{
    public class CreateShopVoucherDto
    {
        [Required(ErrorMessage = "Mã voucher không được để trống")]
        [StringLength(50, ErrorMessage = "Mã voucher không được quá 50 ký tự")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại voucher không được để trống")]
        public VoucherType Type { get; set; }

        [Required(ErrorMessage = "Giá trị voucher không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị voucher phải lớn hơn 0")]
        public decimal Value { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị tối đa phải lớn hơn hoặc bằng 0")]
        public decimal? MaxValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal MinOrderAmount { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int AvailableQuantity { get; set; }
    }
}