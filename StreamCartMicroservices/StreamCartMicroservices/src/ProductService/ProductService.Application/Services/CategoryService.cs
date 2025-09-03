using ProductService.Application.DTOs.Category;
using ProductService.Application.Interfaces;
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
            if (category.ParentCategoryID != Guid.Empty && category.ParentCategoryID != null )
            {
                var existingCateogry = await _categoryRepo.FindOneAsync(x => x.Id == category.ParentCategoryID && x.IsDeleted == false);
                if (existingCateogry == null)
                {
                    result.Success = false;
                    result.Message = "Danh mục cha không tồn tại.";
                    return result;
                }

            }
            //Check Unique Name
            var existingName = await _categoryRepo.FindOneAsync(x => x.CategoryName == category.CategoryName && x.IsDeleted == false);
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

        public async Task<ApiResponse<ListCategoryDTO>> GetAllCategory(FilterCategoryDTO filter)
        {
            var result = new ApiResponse<ListCategoryDTO>()
            {
                Success = true,
                Message = "Lấy danh mục sản phẩm thành công"
            };

            var categories = (ICollection<Category>)await _categoryRepo.GetAllAsync();

            if (filter.IsDeleted.HasValue)
            {
                categories = categories.Where(x => x.IsDeleted == filter.IsDeleted).ToList();
            }

            var fullTree = BuildCategoryTree(categories, null, filter.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.CategoryName))
            {
                fullTree = FilterTreeByName((List<CategoryDetailDTO>)fullTree, filter.CategoryName);
            }

            if (fullTree == null || fullTree.Count == 0)
            {
                result.Success = false;
                result.Message = "Không tìm thấy danh mục";
                return result;
            }

            int pageIndex = filter.PageIndex ?? 1;
            int pageSize = filter.PageSize ?? fullTree.Count;

            var pagedTree = fullTree
                .OrderBy(c => c.CategoryName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            result.Data = new ListCategoryDTO()
            {
                Categories = pagedTree,
                TotalItem = fullTree.Count,
            };

            return result;
        }

        public async Task<ApiResponse<CategoryDetailDTO>> GetCategoryById(Guid id)
        {
            var result = new ApiResponse<CategoryDetailDTO>()
            {
                Success = true,
                Message = "Lấy danh mục ản phẩm thành công"
            };

            Category category = await _categoryRepo.GetCategoryById(id);
            if (category == null) {
                result.Success = false;
                result.Message = "Không tìm thấy danh mục";
                return result;
            }
            CategoryDetailDTO categoryDetailDTO = new CategoryDetailDTO()
            {
                CategoryId = category.Id.ToString(),
                CategoryName = category.CategoryName,
                Description = category.Description,
                IconURL = category.IconURL,
                Slug = category.Slug,
                SubCategories = BuildCategoryTree(category.SubCategories, id,null),
                IsDeleted = category.IsDeleted,
            };
            result.Data = categoryDetailDTO;
            return result;

        }

        public async Task<ApiResponse<bool>> SoftDeleteCategoryAsync(Guid categoryId, string modifier)
        {
            var response = new ApiResponse<bool> { Success = true };
            var category = await _categoryRepo.FindOneAsync(x => x.Id == categoryId);
            if (category == null)
            {
                response.Success = false;
                response.Message = "Danh mục sản phẩm không tồn tại";
                return response;
            }
            if (category.IsDeleted) {
            
                category.Restore(modifier);
                await _categoryRepo.ReplaceAsync(categoryId.ToString(), category);
                response.Message = "Khôi phục danh mục sản phẩm thành công";
                return response;

            }
            category.Delete(modifier.ToString());
            var subCategories = await _categoryRepo.GetAllAsync();
            subCategories = subCategories.Where(x => x.ParentCategoryID == categoryId).ToList();
            foreach (var sub in subCategories)
            {
                sub.ParentCategoryID = null;
                sub.SetModifier(modifier);
            }
            try
            {
                await _categoryRepo.ReplaceAsync(categoryId.ToString(),category);
                foreach (var sub in subCategories)
                {
                await _categoryRepo.ReplaceAsync(sub.Id.    ToString(),sub);
                }

                response.Data = true;
                response.Message = "Đã xóa danh mục sản phẩm thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Xảy ra lỗi khi xóa danh mục " + ex.Message;
            }

            return response;
        }

        public async Task<ApiResponse<Category>> UpdateCategory(UpdateCategoryDTO category, Guid Id)
        {
            var result = new ApiResponse<Category>()
            {
                Success = true,
                Message = "Cập nhật danh mục sản phẩm thành công"
            };
            //Check category
            var existingCategory = await _categoryRepo.GetCategoryById(Id);
            if ( existingCategory == null)
            {
                result.Success = false;
                result.Message = "Danh mục sản phẩm không tồn tại.";
                return result;
            }
            //Check Parent Category
            if (category.ParentCategoryID != Guid.Empty && category.ParentCategoryID != null)
            {
                var existingParentCateogry = await _categoryRepo.FindOneAsync(x => x.Id == category.ParentCategoryID && x.IsDeleted == false);
                if (existingParentCateogry == null)
                {
                    result.Success = false;
                    result.Message = "Danh mục cha không tồn tại.";
                    return result;
                }

            }

            try
            {
                existingCategory.CategoryName = category.CategoryName ?? existingCategory.CategoryName;
                existingCategory.Description = category.Description;
                existingCategory.Slug = category.Slug;
                existingCategory.IconURL = category.IconURL;
                existingCategory.ParentCategoryID = category.ParentCategoryID;
                existingCategory.SetModifier(category.Modifier);
                await _categoryRepo.ReplaceAsync(Id.ToString(), existingCategory);
                result.Data = existingCategory;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Đã xảy ra lỗi khi tạo danh mục sản phẩm";
                return result;
            }

        }

        private ICollection<CategoryDetailDTO> BuildCategoryTree(ICollection<Category> categories, Guid? parent, bool? isDeleted) {
            return categories
        .Where(x => x.ParentCategoryID == parent && (isDeleted == null || x.IsDeleted == isDeleted))
        .OrderBy(c => c.CategoryName)
        .Select(x => new CategoryDetailDTO
        {
            CategoryId = x.Id.ToString(),
            CategoryName = x.CategoryName,
            Description = x.Description,
            IconURL = x.IconURL,
            Slug = x.Slug,
            SubCategories = BuildCategoryTree(categories, x.Id, isDeleted),
            IsDeleted = x.IsDeleted,
        }).ToList();

        }
        private List<CategoryDetailDTO> FilterTreeByName(List<CategoryDetailDTO> categories, string keyword)
        {
            keyword = keyword.ToLower();

            var result = new List<CategoryDetailDTO>();

            foreach (var cat in categories)
            {
                // Lọc các sub trước
                var filteredSubs = FilterTreeByName(cat.SubCategories.ToList(), keyword);

                // Nếu chính nó match hoặc có sub match
                if (cat.CategoryName.ToLower().Contains(keyword) || filteredSubs.Count > 0)
                {
                    result.Add(new CategoryDetailDTO
                    {
                        CategoryId = cat.CategoryId,
                        CategoryName = cat.CategoryName,
                        Description = cat.Description,
                        IconURL = cat.IconURL,
                        Slug = cat.Slug,
                        IsDeleted = cat.IsDeleted,
                        SubCategories = filteredSubs
                    });
                }
            }

            return result;
        }

    }
}
