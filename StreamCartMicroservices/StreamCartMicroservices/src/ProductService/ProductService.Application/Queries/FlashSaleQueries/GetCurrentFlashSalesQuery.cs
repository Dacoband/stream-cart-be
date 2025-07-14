using MediatR;
using ProductService.Application.DTOs.FlashSale;
using Shared.Common.Models;
using System.Collections.Generic;

namespace ProductService.Application.Queries.FlashSaleQueries
{
    public class GetCurrentFlashSalesQuery : IRequest<ApiResponse<List<DetailFlashSaleDTO>>>
    {
        // Empty as we just want current flash sales
    }
}