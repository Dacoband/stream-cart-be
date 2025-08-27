using MediatR;
using Microsoft.Extensions.Logging;
using ShopService.Application.DTOs.Voucher;
using ShopService.Application.Interfaces;
using ShopService.Application.Queries.Voucher;
using ShopService.Domain.Enums;

namespace ShopService.Application.Handlers.Voucher
{
    public class GetAvailableVouchersForCustomerHandler : IRequestHandler<GetAvailableVouchersForCustomerQuery, List<CustomerVoucherResponseDto>>
    {
        private readonly IShopVoucherRepository _voucherRepository;
        private readonly IShopRepository _shopRepository;
        private readonly ILogger<GetAvailableVouchersForCustomerHandler> _logger;

        public GetAvailableVouchersForCustomerHandler(
            IShopVoucherRepository voucherRepository,
            IShopRepository shopRepository,
            ILogger<GetAvailableVouchersForCustomerHandler> logger)
        {
            _voucherRepository = voucherRepository;
            _shopRepository = shopRepository;
            _logger = logger;
        }

        public async Task<List<CustomerVoucherResponseDto>> Handle(GetAvailableVouchersForCustomerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🎫 Getting available vouchers for order amount: {OrderAmount}đ, ShopId: {ShopId}",
                    request.OrderAmount, request.ShopId);

                // Lấy danh sách voucher khả dụng
                var vouchers = await _voucherRepository.GetValidVouchersForOrderAsync((Guid)request.ShopId, request.OrderAmount);

                // Lọc thêm theo type nếu có
                if (request.VoucherType.HasValue)
                {
                    vouchers = vouchers.Where(v => v.Type == request.VoucherType.Value);
                }

                var result = new List<CustomerVoucherResponseDto>();

                foreach (var voucher in vouchers)
                {
                    var discountAmount = voucher.CalculateDiscount(request.OrderAmount);
                    var finalAmount = request.OrderAmount - discountAmount;
                    var discountPercentage = request.OrderAmount > 0 ? (discountAmount / request.OrderAmount) * 100 : 0;

                    var shop = await _shopRepository.GetByIdAsync(voucher.ShopId.ToString());

                    var customerVoucherDto = new CustomerVoucherDto
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
                        RemainingQuantity = voucher.AvailableQuantity - voucher.UsedQuantity,
                        ShopName = shop?.ShopName ?? "Unknown Shop",
                        ShopImageUrl = shop?.CoverImageURL ?? ""
                    };

                    var voucherResponse = new CustomerVoucherResponseDto
                    {
                        Voucher = customerVoucherDto,
                        DiscountAmount = discountAmount,
                        FinalAmount = finalAmount,
                        DiscountPercentage = discountPercentage,
                        DiscountMessage = GenerateDiscountMessage(voucher, discountAmount)
                    };

                    result.Add(voucherResponse);
                }
                if (request.SortByDiscountDesc)
                {
                    result = result.OrderByDescending(v => v.DiscountAmount)
                                  .ThenByDescending(v => v.DiscountPercentage)
                                  .ToList();
                }
                else
                {
                    result = result.OrderBy(v => v.DiscountAmount)
                                  .ThenBy(v => v.DiscountPercentage)
                                  .ToList();
                }
                result = result.Take(request.Limit).ToList();

                _logger.LogInformation("✅ Found {Count} available vouchers for order amount {OrderAmount}đ",
                    result.Count, request.OrderAmount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting available vouchers for customer");
                throw;
            }
        }

        private static string GenerateDiscountMessage(Domain.Entities.ShopVoucher voucher, decimal discountAmount)
        {
            return voucher.Type switch
            {
                VoucherType.Percentage => $"Giảm {voucher.Value}% (Tiết kiệm {discountAmount:N0}đ)",
                VoucherType.FixedAmount => $"Giảm {discountAmount:N0}đ",
                _ => $"Tiết kiệm {discountAmount:N0}đ"
            };
        }
    }
}