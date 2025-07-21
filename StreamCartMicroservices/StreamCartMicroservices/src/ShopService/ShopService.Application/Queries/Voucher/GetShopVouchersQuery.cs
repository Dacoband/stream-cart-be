using MediatR;
using Shared.Common.Domain.Bases;
using ShopService.Application.DTOs.Voucher;
using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries.Voucher
{
    public class GetShopVouchersQuery : IRequest<PagedResult<ShopVoucherDto>>
    {
        public Guid ShopId { get; set; }
        public bool? IsActive { get; set; }
        public VoucherType? Type { get; set; }
        public bool? IsExpired { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
