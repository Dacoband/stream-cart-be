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
    public class GetAllCategoryHanlder : IRequestHandler<GetAllCategoryQuery, ApiResponse<ListCategoryDTO>>
    {
        private readonly ICategoryService _categoryService;

        public GetAllCategoryHanlder(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public async Task<ApiResponse<ListCategoryDTO>> Handle(GetAllCategoryQuery request, CancellationToken cancellationToken)
        {
            var filter = new FilterCategoryDTO
            {
                CategoryName = request.CategoryName,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                IsDeleted = request.IsDeleted,
            };

            var result = await _categoryService.GetAllCategory(filter);
            return result;
        }
    }
}
