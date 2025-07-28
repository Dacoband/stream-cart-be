using MediatR;
using ShopService.Application.Commands.Voucher;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class CreateShopVoucherHandler : IRequestHandler<CreateShopVoucherCommand, ShopVoucherDto>
    {
        private readonly IShopVoucherRepository _voucherRepository;
        private readonly IShopRepository _shopRepository;

        public CreateShopVoucherHandler(
            IShopVoucherRepository voucherRepository,
            IShopRepository shopRepository)
        {
            _voucherRepository = voucherRepository;
            _shopRepository = shopRepository;
        }

        public async Task<ShopVoucherDto> Handle(CreateShopVoucherCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra shop tồn tại
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());
            if (shop == null)
                throw new ArgumentException($"Shop với ID {request.ShopId} không tồn tại");

            // Kiểm tra mã voucher đã tồn tại chưa
            var isCodeUnique = await _voucherRepository.IsCodeUniqueAsync(request.Code);
            if (!isCodeUnique)
                throw new InvalidOperationException($"Mã voucher '{request.Code}' đã tồn tại");

            // Tạo voucher mới
            var voucher = new ShopVoucher(
                request.ShopId,
                request.Code,
                request.Description,
                request.Type,
                request.Value,
                request.MinOrderAmount,
                request.StartDate,
                request.EndDate,
                request.AvailableQuantity,
                request.MaxValue);

            voucher.SetCreator(request.CreatedBy);

            // Lưu vào database
            await _voucherRepository.InsertAsync(voucher);

            // Trả về DTO
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
                ShopName = shop.ShopName
            };
        }
    }
}