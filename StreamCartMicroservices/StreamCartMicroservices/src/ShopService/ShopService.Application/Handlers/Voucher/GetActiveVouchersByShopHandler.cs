using MediatR;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Voucher;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class GetActiveVouchersByShopHandler : IRequestHandler<GetActiveVouchersByShopQuery, List<ShopVoucherDto>>
    {
        private readonly IShopVoucherRepository _voucherRepository;
        private readonly IShopRepository _shopRepository;

        public GetActiveVouchersByShopHandler(
            IShopVoucherRepository voucherRepository,
            IShopRepository shopRepository)
        {
            _voucherRepository = voucherRepository;
            _shopRepository = shopRepository;
        }

        public async Task<List<ShopVoucherDto>> Handle(GetActiveVouchersByShopQuery request, CancellationToken cancellationToken)
        {
            var vouchers = await _voucherRepository.GetActiveVouchersByShopAsync(request.ShopId);
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());

            return vouchers.Select(v => new ShopVoucherDto
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
            }).ToList();
        }
    }
}