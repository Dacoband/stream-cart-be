using MediatR;
using ShopService.Application.Commands.Voucher;
using ShopService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class DeleteShopVoucherHandler : IRequestHandler<DeleteShopVoucherCommand, bool>
    {
        private readonly IShopVoucherRepository _voucherRepository;

        public DeleteShopVoucherHandler(IShopVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<bool> Handle(DeleteShopVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByIdAsync(request.Id.ToString());
            if (voucher == null)
                throw new ArgumentException($"Voucher với ID {request.Id} không tồn tại");

            voucher.SoftDelete(request.DeletedBy);
            await _voucherRepository.ReplaceAsync(voucher.Id.ToString(), voucher);

            return true;
        }
    }
}