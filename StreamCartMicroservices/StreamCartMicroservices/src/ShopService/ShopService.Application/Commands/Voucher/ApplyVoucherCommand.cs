using MediatR;
using ShopService.Application.DTOs.Voucher;
using System;

namespace ShopService.Application.Commands.Voucher
{
    public class ApplyVoucherCommand : IRequest<VoucherApplicationDto>
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ShopId { get; set; }
    }
}