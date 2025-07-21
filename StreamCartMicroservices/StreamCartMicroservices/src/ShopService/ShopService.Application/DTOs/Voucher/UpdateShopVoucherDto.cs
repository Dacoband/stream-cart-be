using System;
using System.ComponentModel.DataAnnotations;

namespace ShopService.Application.DTOs.Voucher
{
    public class UpdateShopVoucherDto
    {
        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị voucher phải lớn hơn 0")]
        public decimal? Value { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị tối đa phải lớn hơn hoặc bằng 0")]
        public decimal? MaxValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal? MinOrderAmount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int? AvailableQuantity { get; set; }
    }
}