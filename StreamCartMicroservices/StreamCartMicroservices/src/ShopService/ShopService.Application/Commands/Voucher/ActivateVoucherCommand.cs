using MediatR;
using System;

namespace ShopService.Application.Commands.Voucher
{
    public class ActivateVoucherCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}