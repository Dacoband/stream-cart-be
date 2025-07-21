using MediatR;
using ShopService.Application.DTOs.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries.Voucher
{
    public class ValidateVoucherQuery : IRequest<VoucherValidationDto>
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public Guid? ShopId { get; set; }
    }
}
