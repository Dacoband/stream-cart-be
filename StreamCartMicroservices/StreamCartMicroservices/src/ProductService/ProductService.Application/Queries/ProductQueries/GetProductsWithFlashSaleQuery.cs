using MediatR;
using ProductService.Application.DTOs;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.ProductQueries
{
    public class GetProductsWithFlashSaleQuery : IRequest<ApiResponse<IEnumerable<ProductDto>>>
    {
    }
}
