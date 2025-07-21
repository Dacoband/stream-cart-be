using MediatR;
using ShopService.Application.Commands.Voucher;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class ApplyVoucherHandler : IRequestHandler<ApplyVoucherCommand, VoucherApplicationDto>
    {
        private readonly IShopVoucherRepository _voucherRepository;

        public ApplyVoucherHandler(IShopVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<VoucherApplicationDto> Handle(ApplyVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByCodeAsync(request.Code);

            if (voucher == null)
            {
                return new VoucherApplicationDto
                {
                    IsApplied = false,
                    Message = "Mã voucher không tồn tại",
                    DiscountAmount = 0,
                    FinalAmount = request.OrderAmount
                };
            }

            if (request.ShopId.HasValue && voucher.ShopId != request.ShopId.Value)
            {
                return new VoucherApplicationDto
                {
                    IsApplied = false,
                    Message = "Voucher không áp dụng cho shop này",
                    DiscountAmount = 0,
                    FinalAmount = request.OrderAmount
                };
            }

            if (!voucher.CanApplyToOrder(request.OrderAmount))
            {
                var message = !voucher.IsVoucherValid()
                    ? "Voucher đã hết hạn hoặc không còn hiệu lực"
                    : $"Đơn hàng phải có giá trị tối thiểu {voucher.MinOrderAmount:C}";

                return new VoucherApplicationDto
                {
                    IsApplied = false,
                    Message = message,
                    DiscountAmount = 0,
                    FinalAmount = request.OrderAmount
                };
            }

            // Áp dụng voucher
            var discountAmount = voucher.CalculateDiscount(request.OrderAmount);
            voucher.Use(); // Tăng số lượng đã sử dụng

            await _voucherRepository.ReplaceAsync(voucher.Id.ToString(), voucher);

            return new VoucherApplicationDto
            {
                IsApplied = true,
                Message = "Áp dụng voucher thành công",
                DiscountAmount = discountAmount,
                FinalAmount = request.OrderAmount - discountAmount,
                VoucherId = voucher.Id,
                VoucherCode = voucher.Code,
                AppliedAt = DateTime.UtcNow
            };
        }
    }
}