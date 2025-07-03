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
    public class GetAllCategoryQuery : IRequest<ApiResponse<ListCategoryDTO>>
    {
        public string? CategoryName { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; } 
        public bool? IsDeleted { get; set; }
    }
}
