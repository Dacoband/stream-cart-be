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
    public class GetDetailFlashSaleHandler : IRequestHandler<GetDetailFlashSaleQuery, ApiResponse<DetailFlashSaleDTO>>
    {
        private readonly IFlashSaleService _flashSaleService;
        public GetDetailFlashSaleHandler(IFlashSaleService flashSaleService)
        {
            _flashSaleService = flashSaleService;
        }
        public async Task<ApiResponse<DetailFlashSaleDTO>> Handle(GetDetailFlashSaleQuery request, CancellationToken cancellationToken)
        {
            return await _flashSaleService.GetFlashSaleById(request.FlashSaleId);
        }
    }
}
