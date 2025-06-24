using MediatR;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.FlashSaleQueries;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.FlashSaleHandlers
{
    public class GetAllFlashSaleHandler : IRequestHandler<GetAllFlashSaleQuery, ApiResponse<List<DetailFlashSaleDTO>>>
    {
        private readonly IFlashSaleService _flashSaleService;
        public GetAllFlashSaleHandler(IFlashSaleService flashSaleService)
        {
            _flashSaleService = flashSaleService;
        }
        public Task<ApiResponse<List<DetailFlashSaleDTO>>> Handle(GetAllFlashSaleQuery request, CancellationToken cancellationToken)
        {
            FilterFlashSaleDTO filter = new FilterFlashSaleDTO()
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,
                OrderBy = request.OrderBy,
                OrderDirection = request.OrderDirection,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
            };
            return _flashSaleService.FilterFlashSale(filter);
        }
    }
}
