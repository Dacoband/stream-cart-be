using MediatR;
using ProductService.Application.DTOs.Category;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.CategoryQueries;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CategoryHandlers
{
    public class GetDetailCategoryHandler : IRequestHandler<GetDetailCategoryQuery, ApiResponse<CategoryDetailDTO>>
    {
        private readonly ICategoryService _categoryService;
        public GetDetailCategoryHandler(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<ApiResponse<CategoryDetailDTO>> Handle(GetDetailCategoryQuery request, CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetCategoryById(request.Id);
            return result;
        }
    }
}
