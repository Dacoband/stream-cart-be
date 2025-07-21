using MediatR;
using ShopService.Application.DTOs.Voucher;
using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Commands.Voucher
{
    public class CreateShopVoucherCommand : IRequest<ShopVoucherDto>
    {
        public Guid ShopId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VoucherType Type { get; set; }
        public decimal Value { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AvailableQuantity { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}
