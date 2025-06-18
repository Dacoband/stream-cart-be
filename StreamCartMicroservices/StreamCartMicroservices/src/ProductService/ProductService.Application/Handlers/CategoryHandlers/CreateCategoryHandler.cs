    using MediatR;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.Interface;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Repositories;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CategoryHandlers
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<Category>>
    {
        private readonly ICategoryService _categoryService;
        public CreateCategoryHandler(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<ApiResponse<Category>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            Category category = new Category( request.CategoryName, request.Description, request.IconURL, request.Slug, request.ParentCategoryID);
            category.SetCreator(request.CreatedBy);
            category.SetModifier(request.CreatedBy);
            return await _categoryService.CreateCategory(category);

        }
    }
}
