using MediatR;
using ShopService.Application.Commands.Voucher;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class UpdateShopVoucherHandler : IRequestHandler<UpdateShopVoucherCommand, ShopVoucherDto>
    {
        private readonly IShopVoucherRepository _voucherRepository;
        private readonly IShopRepository _shopRepository;

        public UpdateShopVoucherHandler(
            IShopVoucherRepository voucherRepository,
            IShopRepository shopRepository)
        {
            _voucherRepository = voucherRepository;
            _shopRepository = shopRepository;
        }

        public async Task<ShopVoucherDto> Handle(UpdateShopVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByIdAsync(request.Id.ToString());
            if (voucher == null)
                throw new ArgumentException($"Voucher với ID {request.Id} không tồn tại");

            // Cập nhật các trường nếu có giá trị
            if (!string.IsNullOrEmpty(request.Description))
                voucher.UpdateDescription(request.Description);

            if (request.Value.HasValue)
                voucher.UpdateValue(request.Value.Value, request.MaxValue);

            if (request.MinOrderAmount.HasValue)
                voucher.UpdateMinOrderAmount(request.MinOrderAmount.Value);

            if (request.StartDate.HasValue && request.EndDate.HasValue)
                voucher.UpdateDates(request.StartDate.Value, request.EndDate.Value);

            if (request.AvailableQuantity.HasValue)
                voucher.UpdateQuantity(request.AvailableQuantity.Value);

            voucher.SetModifier(request.UpdatedBy);

            await _voucherRepository.ReplaceAsync(voucher.Id.ToString(), voucher);

            // Lấy thông tin shop
            var shop = await _shopRepository.GetByIdAsync(voucher.ShopId.ToString());

            return new ShopVoucherDto
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
                IsValid = voucher.IsVoucherValid(),
                CreatedAt = voucher.CreatedAt,
                CreatedBy = voucher.CreatedBy,
                LastModifiedAt = voucher.LastModifiedAt,
                LastModifiedBy = voucher.LastModifiedBy,
                ShopName = shop?.ShopName
            };
        }
    }
}