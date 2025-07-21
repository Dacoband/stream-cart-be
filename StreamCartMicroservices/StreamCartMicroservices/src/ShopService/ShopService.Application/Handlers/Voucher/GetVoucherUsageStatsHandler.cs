using MediatR;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Voucher;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShopService.Application.Handlers.Voucher
{
    public class GetVoucherUsageStatsHandler : IRequestHandler<GetVoucherUsageStatsQuery, VoucherUsageStatsDto>
    {
        private readonly IShopVoucherRepository _voucherRepository;

        public GetVoucherUsageStatsHandler(IShopVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<VoucherUsageStatsDto> Handle(GetVoucherUsageStatsQuery request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByIdAsync(request.VoucherId.ToString());
            if (voucher == null)
                throw new ArgumentException($"Voucher với ID {request.VoucherId} không tồn tại");

            var usageStats = await _voucherRepository.GetUsageStatisticsAsync(request.VoucherId);
            var remainingQuantity = voucher.AvailableQuantity - voucher.UsedQuantity;
            var usagePercentage = voucher.AvailableQuantity > 0
                ? (decimal)voucher.UsedQuantity / voucher.AvailableQuantity * 100
                : 0;

            return new VoucherUsageStatsDto
            {
                VoucherId = voucher.Id,
                Code = voucher.Code,
                TotalQuantity = voucher.AvailableQuantity,
                UsedQuantity = voucher.UsedQuantity,
                RemainingQuantity = remainingQuantity,
                UsagePercentage = usagePercentage,
                FirstUsedAt = voucher.UsedQuantity > 0 ? voucher.CreatedAt : null,
                LastUsedAt = voucher.LastModifiedAt,
                TotalDiscountGiven = 0, // This would need additional tracking
                UniqueUsersCount = voucher.UsedQuantity // Simplified - would need user tracking
            };
        }
    }
}