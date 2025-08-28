using ShopService.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ShopService.Application.DTOs.Voucher
{
    public class CustomerVoucherRequestDto
    {
       
        [Required(ErrorMessage = "Số tiền đơn hàng không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền đơn hàng phải lớn hơn 0")]
        public decimal OrderAmount { get; set; }
        public Guid? ShopId { get; set; }
        [Range(1, 50, ErrorMessage = "Số lượng voucher phải từ 1 đến 50")]
        //public int Limit { get; set; } = 10;
        //public VoucherType? VoucherType { get; set; }
        public bool SortByDiscountDesc { get; set; } = true;
    }
}