using MediatR;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.FlashSaleQueries;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.FlashSaleQueryHandlers
{
    public class GetCurrentFlashSalesQueryHandler : IRequestHandler<GetCurrentFlashSalesQuery, ApiResponse<List<DetailFlashSaleDTO>>>
    {
        private readonly IFlashSaleService _flashSaleService;

        public GetCurrentFlashSalesQueryHandler(IFlashSaleService flashSaleService)
        {
            _flashSaleService = flashSaleService;
        }

        public async Task<ApiResponse<List<DetailFlashSaleDTO>>> Handle(GetCurrentFlashSalesQuery request, CancellationToken cancellationToken)
        {
            // ✅ FIX: Sử dụng timezone SE Asia để so sánh chính xác
            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

            // ✅ Lấy tất cả FlashSale active mà không giới hạn thời gian
            var filter = new FilterFlashSaleDTO
            {
                IsActive = null, // Lấy tất cả trước, sẽ lọc sau
                StartDate = null,
                EndDate = null,
                OrderBy = FlashSaleOrderBy.StartDate,
                OrderDirection = OrderDirection.Asc,
                PageIndex = 0,
                PageSize = 100
            };

            var result = await _flashSaleService.FilterFlashSale(filter);

            if (result.Success && result.Data != null)
            {
                // ✅ FIX: Lọc FlashSale đang diễn ra với timezone đúng
                var currentFlashSales = result.Data.Where(fs =>
                {
                    // fs.StartTime và fs.EndTime đã được convert sang SE Asia timezone trong FilterFlashSale
                    return fs.StartTime <= nowLocal &&
                           fs.EndTime >= nowLocal &&
                           fs.IsActive;
                }).ToList();

                result.Data = currentFlashSales;
                result.Message = $"Tìm thấy {currentFlashSales.Count} FlashSale đang diễn ra";

                // ✅ Debug information
                var debugInfo = new
                {
                    NowUtc = nowUtc,
                    NowLocal = nowLocal,
                    TotalFlashSales = result.Data?.Count ?? 0,
                    FilteredFlashSales = currentFlashSales.Select(fs => new
                    {
                        fs.Id,
                        fs.ProductId,
                        fs.StartTime,
                        fs.EndTime,
                        fs.IsActive,
                        IsCurrentlyActive = fs.StartTime <= nowLocal && fs.EndTime >= nowLocal && fs.IsActive
                    }).ToList()
                };

                result.Message += $" (Debug: UTC={nowUtc:yyyy-MM-dd HH:mm:ss}, Local={nowLocal:yyyy-MM-dd HH:mm:ss})";
            }

            return result;
        }
    }
}