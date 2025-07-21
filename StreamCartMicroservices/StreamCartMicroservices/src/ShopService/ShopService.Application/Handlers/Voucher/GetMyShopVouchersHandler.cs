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
    public class GetMyShopVouchersHandler : IRequestHandler<GetMyShopVouchersQuery, PagedResult<ShopVoucherDto>>
    {
        private readonly IShopVoucherRepository _voucherRepository;
        private readonly IShopRepository _shopRepository;

        public GetMyShopVouchersHandler(
            IShopVoucherRepository voucherRepository,
            IShopRepository shopRepository)
        {
            _voucherRepository = voucherRepository;
            _shopRepository = shopRepository;
        }

        public async Task<PagedResult<ShopVoucherDto>> Handle(GetMyShopVouchersQuery request, CancellationToken cancellationToken)
        {
            // Lấy shops của user
            var userShops = await _shopRepository.GetShopsByAccountIdAsync(request.UserId);
            var shopIds = userShops.Select(s => s.Id).ToList();

            if (!shopIds.Any())
            {
                return new PagedResult<ShopVoucherDto>
                {
                    Items = new List<ShopVoucherDto>(),
                    CurrentPage = request.Filter.PageNumber,
                    PageSize = request.Filter.PageSize,
                    TotalCount = 0
                };
            }

            // Get vouchers for all user's shops
            var allVouchers = new List<ShopVoucherDto>();
            foreach (var shopId in shopIds)
            {
                var pagedVouchers = await _voucherRepository.GetVouchersPagedAsync(
                    shopId,
                    request.Filter.IsActive,
                    request.Filter.Type,
                    request.Filter.IsExpired,
                    1,
                    int.MaxValue); // Get all vouchers first

                var shop = userShops.First(s => s.Id == shopId);
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
                    ShopName = shop.ShopName
                });

                allVouchers.AddRange(voucherDtos);
            }

            // Apply additional filtering
            var filteredVouchers = allVouchers.AsQueryable();

            if (!string.IsNullOrEmpty(request.Filter.SearchTerm))
            {
                filteredVouchers = filteredVouchers.Where(v =>
                    v.Code.Contains(request.Filter.SearchTerm) ||
                    v.Description.Contains(request.Filter.SearchTerm) ||
                    (v.ShopName != null && v.ShopName.Contains(request.Filter.SearchTerm)));
            }

            if (request.Filter.StartDate.HasValue)
            {
                filteredVouchers = filteredVouchers.Where(v => v.StartDate >= request.Filter.StartDate.Value);
            }

            if (request.Filter.EndDate.HasValue)
            {
                filteredVouchers = filteredVouchers.Where(v => v.EndDate <= request.Filter.EndDate.Value);
            }

            // Apply pagination
            var totalCount = filteredVouchers.Count();
            var items = filteredVouchers
                .OrderByDescending(v => v.CreatedAt)
                .Skip((request.Filter.PageNumber - 1) * request.Filter.PageSize)
                .Take(request.Filter.PageSize)
                .ToList();

            return new PagedResult<ShopVoucherDto>
            {
                Items = items,
                CurrentPage = request.Filter.PageNumber,
                PageSize = request.Filter.PageSize,
                TotalCount = totalCount
            };
        }
    }
}