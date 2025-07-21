using MediatR;
using ShopService.Application.Commands.Voucher;
using ShopService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class ActivateVoucherHandler : IRequestHandler<ActivateVoucherCommand, bool>
    {
        private readonly IShopVoucherRepository _voucherRepository;

        public ActivateVoucherHandler(IShopVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<bool> Handle(ActivateVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByIdAsync(request.Id.ToString());
            if (voucher == null)
                throw new ArgumentException($"Voucher với ID {request.Id} không tồn tại");

            voucher.Activate(request.ModifiedBy);
            await _voucherRepository.ReplaceAsync(voucher.Id.ToString(), voucher);

            return true;
        }
    }
}