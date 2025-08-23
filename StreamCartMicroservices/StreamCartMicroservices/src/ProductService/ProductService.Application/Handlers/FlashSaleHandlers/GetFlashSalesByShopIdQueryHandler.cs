using MediatR;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.FlashSaleQueries;
using Shared.Common.Models;

namespace ProductService.Application.Handlers.FlashSaleHandlers
{
    public class GetFlashSalesByShopIdQueryHandler : IRequestHandler<GetFlashSalesByShopIdQuery, ApiResponse<List<DetailFlashSaleDTO>>>
    {
        private readonly IFlashSaleService _flashSaleService;

        public GetFlashSalesByShopIdQueryHandler(IFlashSaleService flashSaleService)
        {
            _flashSaleService = flashSaleService;
        }

        public async Task<ApiResponse<List<DetailFlashSaleDTO>>> Handle(GetFlashSalesByShopIdQuery request, CancellationToken cancellationToken)
        {
            return await _flashSaleService.GetFlashSalesByShopIdAsync(request.ShopId, request.Filter);
        }
    }
}