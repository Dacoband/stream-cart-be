using MediatR;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.DTOs.Category;
using ProductService.Application.Interface;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CategoryHandlers
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<Category>>
    {
        private readonly ICategoryService _categoryService;
        public UpdateCategoryHandler(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<ApiResponse<Category>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            
            UpdateCategoryDTO updateCategoryDTO = new UpdateCategoryDTO()
            {
                CategoryName = request.CategoryName,
                Description = request.Description,
                IconURL = request.IconURL,
                Slug = request.Slug,
                ParentCategoryID = request.ParentCategoryID,
                Modifier = request.LastModifiedBy,
            };

            return await _categoryService.UpdateCategory(updateCategoryDTO,request.Id);

        }
    }
}
