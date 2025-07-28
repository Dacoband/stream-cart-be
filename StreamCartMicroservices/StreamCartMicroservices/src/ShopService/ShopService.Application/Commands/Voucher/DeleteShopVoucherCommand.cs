using MediatR;
using System;

namespace ShopService.Application.Commands.Voucher
{
    public class DeleteShopVoucherCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string DeletedBy { get; set; } = string.Empty;
    }
}