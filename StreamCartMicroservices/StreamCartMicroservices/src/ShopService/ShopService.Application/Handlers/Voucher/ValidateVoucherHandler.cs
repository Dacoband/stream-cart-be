using MediatR;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Voucher;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class ValidateVoucherHandler : IRequestHandler<ValidateVoucherQuery, VoucherValidationDto>
    {
        private readonly IShopVoucherRepository _voucherRepository;

        public ValidateVoucherHandler(IShopVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<VoucherValidationDto> Handle(ValidateVoucherQuery request, CancellationToken cancellationToken)
        {
            // Tìm voucher theo mã
            var voucher = await _voucherRepository.GetByCodeAsync(request.Code);

            if (voucher == null)
            {
                return new VoucherValidationDto
                {
                    IsValid = false,
                    Message = "Mã voucher không tồn tại",
                    DiscountAmount = 0,
                    FinalAmount = request.OrderAmount
                };
            }

            // Kiểm tra shop nếu có
            if (request.ShopId.HasValue && voucher.ShopId != request.ShopId.Value)
            {
                return new VoucherValidationDto
                {
                    IsValid = false,
                    Message = "Voucher không áp dụng cho shop này",
                    DiscountAmount = 0,
                    FinalAmount = request.OrderAmount
                };
            }

            // Kiểm tra voucher có thể áp dụng cho đơn hàng không
            if (!voucher.CanApplyToOrder(request.OrderAmount))
            {
                var message = !voucher.IsVoucherValid()
                    ? "Voucher đã hết hạn hoặc không còn hiệu lực"
                    : $"Đơn hàng phải có giá trị tối thiểu {voucher.MinOrderAmount:C}";

                return new VoucherValidationDto
                {
                    IsValid = false,
                    Message = message,
                    DiscountAmount = 0,
                    FinalAmount = request.OrderAmount
                };
            }

            // Tính toán giảm giá
            var discountAmount = voucher.CalculateDiscount(request.OrderAmount);
            var finalAmount = request.OrderAmount - discountAmount;

            return new VoucherValidationDto
            {
                IsValid = true,
                Message = "Voucher hợp lệ",
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                Voucher = new ShopVoucherDto
                {
                    Id = voucher.Id,
                    ShopId = voucher.ShopId,
                    Code = voucher.Code,
                    Description = voucher.Description,
                    Type = voucher.Type,
                    Value = voucher.Value,
                    MaxValue = voucher.MaxValue,
                    MinOrderAmount = voucher.MinOrderAmount,
                    StartDate = voucher.StartDate,
                    EndDate = voucher.EndDate,
                    AvailableQuantity = voucher.AvailableQuantity,
                    UsedQuantity = voucher.UsedQuantity,
                    IsActive = voucher.IsActive,
                    IsValid = voucher.IsVoucherValid()
                }
            };
        }
    }
}