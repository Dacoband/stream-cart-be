using ProductService.Application.Interface;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;
        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }
        public async Task<ApiResponse<Category>> CreateCategory(Category category)
        {
            var result = new ApiResponse<Category>()
            {
                Success = true,
                Message = "Tạo danh mục sản phẩm thành công"
            };
            //Check Parent Category
            if (category.Id != Guid.Empty)
            {
                var existingCateogry = _categoryRepo.FindOneAsync(x => x.Id == category.ParentCategoryID && x.IsDeleted == false);
                if (existingCateogry == null)
                {
                    result.Success = false;
                    result.Message = "Danh mục cha không tồn tại.";
                    return result;
                }

            }
            //Check Unique Name
            var existingName = _categoryRepo.FindOneAsync(x => x.CategoryName == category.CategoryName && x.IsDeleted == false);
            if (existingName != null)
            {
                result.Success = false;
                result.Message = "Danh mục sản phẩm đã tồn tại";
                return result;
            }

            

            try
            {
                await _categoryRepo.InsertAsync(category);
                result.Data = category;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Đã xảy ra lỗi khi tạo danh mục sản phẩm";
                return result;


            }
        }
    }
}
