using MediatR;
using ProductService.Application.DTOs.Category;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.CategoryQueries
{
    public class GetDetailCategoryQuery : IRequest<ApiResponse<CategoryDetailDTO>>
    {
        public Guid Id { get; set; }
    }
}
