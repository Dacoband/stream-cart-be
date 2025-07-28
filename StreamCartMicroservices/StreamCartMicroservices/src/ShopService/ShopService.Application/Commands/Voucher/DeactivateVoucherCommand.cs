using MediatR;
using System;

namespace ShopService.Application.Commands.Voucher
{
    public class DeactivateVoucherCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}