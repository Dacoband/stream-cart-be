using MediatR;
using Shared.Common.Domain.Bases;
using ShopService.Application.DTOs.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries.Voucher
{
    public class GetMyShopVouchersQuery : IRequest<PagedResult<ShopVoucherDto>>
    {
        public Guid UserId { get; set; }
        public VoucherFilterDto Filter { get; set; } = new();
    }
}
