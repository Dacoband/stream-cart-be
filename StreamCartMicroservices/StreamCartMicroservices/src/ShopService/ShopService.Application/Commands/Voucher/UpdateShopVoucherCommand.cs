using MediatR;
using ShopService.Application.DTOs.Voucher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Commands.Voucher
{
    public class UpdateShopVoucherCommand : IRequest<ShopVoucherDto>
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public decimal? Value { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AvailableQuantity { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
