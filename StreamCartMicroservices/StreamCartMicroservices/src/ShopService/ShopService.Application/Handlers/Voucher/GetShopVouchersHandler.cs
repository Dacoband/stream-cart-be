using MediatR;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Voucher;
using Shared.Common.Domain.Bases;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class GetShopVouchersHandler : IRequestHandler<GetShopVouchersQuery, PagedResult<ShopVoucherDto>>
    {
        private readonly IShopVoucherRepository _voucherRepository;
        private readonly IShopRepository _shopRepository;

        public GetShopVouchersHandler(
            IShopVoucherRepository voucherRepository,
            IShopRepository shopRepository)
        {
            _voucherRepository = voucherRepository;
            _shopRepository = shopRepository;
        }

        public async Task<PagedResult<ShopVoucherDto>> Handle(GetShopVouchersQuery request, CancellationToken cancellationToken)
        {
            // Lấy danh sách voucher phân trang
            var pagedVouchers = await _voucherRepository.GetVouchersPagedAsync(
                request.ShopId,
                request.IsActive,
                request.Type,
                request.IsExpired,
                request.PageNumber,
                request.PageSize);

            // Lấy thông tin shop
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());

            // Chuyển đổi sang DTO
            var voucherDtos = pagedVouchers.Items.Select(v => new ShopVoucherDto
            {
                Id = v.Id,
                ShopId = v.ShopId,
                Code = v.Code,
                Description = v.Description,
                Type = v.Type,
                Value = v.Value,
                MaxValue = v.MaxValue,
                MinOrderAmount = v.MinOrderAmount,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                AvailableQuantity = v.AvailableQuantity,
                UsedQuantity = v.UsedQuantity,
                IsActive = v.IsActive,
                IsValid = v.IsVoucherValid(),
                CreatedAt = v.CreatedAt,
                CreatedBy = v.CreatedBy,
                LastModifiedAt = v.LastModifiedAt,
                LastModifiedBy = v.LastModifiedBy,
                ShopName = shop?.ShopName
            });

            return new PagedResult<ShopVoucherDto>
            {
                Items = voucherDtos,
                CurrentPage = pagedVouchers.CurrentPage,
                PageSize = pagedVouchers.PageSize,
                TotalCount = pagedVouchers.TotalCount
            };
        }
    }
}