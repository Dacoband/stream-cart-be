using MediatR;
using ProductService.Application.DTOs.FlashSale;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.FlashSaleQueries
{
    public class GetFlashSalesByShopIdQuery : IRequest<ApiResponse<List<DetailFlashSaleDTO>>>
    {
        public string ShopId { get; set; }
        public FilterFlashSaleDTO Filter { get; set; }
    }
}
