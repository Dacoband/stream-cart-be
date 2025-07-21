using MediatR;
using ShopService.Application.DTOs.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries.Voucher
{
    public class GetVoucherUsageStatsQuery : IRequest<VoucherUsageStatsDto>
    {
        public Guid VoucherId { get; set; }
    }
}
