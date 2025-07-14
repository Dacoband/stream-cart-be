using MediatR;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.FlashSaleQueries;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
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
            var now = DateTime.UtcNow;

            var filter = new FilterFlashSaleDTO
            {
                IsActive = true,
                StartDate = now.AddMinutes(-1), // Include just started sales
                EndDate = now.AddDays(1),      // Include ongoing sales
                OrderBy = FlashSaleOrderBy.StartDate,
                OrderDirection = OrderDirection.Asc,
                PageIndex = 0,
                PageSize = 100              // Adjust as needed
            };

            return await _flashSaleService.FilterFlashSale(filter);
        }
    }
}