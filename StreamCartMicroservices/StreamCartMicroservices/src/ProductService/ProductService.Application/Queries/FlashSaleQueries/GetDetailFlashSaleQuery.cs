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
    public class GetDetailFlashSaleQuery : IRequest<ApiResponse<DetailFlashSaleDTO>>
    {
        public string FlashSaleId { get; set; }
    }
}
