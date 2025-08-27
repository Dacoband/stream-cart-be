using MediatR;
using ShopService.Application.DTOs.Voucher;
using ShopService.Domain.Enums;

namespace ShopService.Application.Queries.Voucher
{
    public class GetAvailableVouchersForCustomerQuery : IRequest<List<CustomerVoucherResponseDto>>
    {
        public decimal OrderAmount { get; set; }
        public Guid? ShopId { get; set; }
        //public int Limit { get; set; } = 10;
        //public VoucherType? VoucherType { get; set; }
        public bool SortByDiscountDesc { get; set; } = true;
    }
}