using MediatR;
using ProductService.Application.Commands.CategoryCommands;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CategoryHandlers
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse<bool>>
    {
        private readonly ICategoryService _categoryService;
        public DeleteCategoryHandler(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public async Task<ApiResponse<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            return await _categoryService.SoftDeleteCategoryAsync(request.CategoryId, request.Modifier);
        }
    }
}
