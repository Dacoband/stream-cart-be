using ProductService.Application.DTOs.Category;
using ProductService.Domain.Entities;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interface
{
    public interface ICategoryService
    {
        public Task<ApiResponse<Category>> CreateCategory(Category category);
        public Task<ApiResponse<ICollection<CategoryDetailDTO>>> GetAllCategory(FilterCategoryDTO filter);
        public Task<ApiResponse<CategoryDetailDTO>> GetCategoryById(Guid id);
        public Task<ApiResponse<Category>> UpdateCategory(UpdateCategoryDTO category, Guid Id);
        Task<ApiResponse<bool>> SoftDeleteCategoryAsync(Guid categoryId, string modifier);


    }
}
