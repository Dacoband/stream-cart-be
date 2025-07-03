using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string createdBy);
        Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto, string updatedBy);
        Task<bool> DeleteProductAsync(Guid id, string deletedBy);
        Task<ProductDto?> GetProductByIdAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(bool activeOnly = false);
        Task<PagedResult<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize, ProductSortOption sortOption, bool activeOnly, Guid? shopId, Guid? categoryId);
        Task<IEnumerable<ProductDto>> GetProductsByShopIdAsync(Guid shopId, bool activeOnly = false);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(Guid categoryId, bool activeOnly = false);
        Task<IEnumerable<ProductDto>> GetBestSellingProductsAsync(int count, Guid? shopId, Guid? categoryId);
        Task<ProductDto> UpdateProductStatusAsync(Guid id, bool isActive, string updatedBy);
        Task<ProductDto> UpdateProductStockAsync(Guid id, int quantity, string updatedBy);
        Task<bool> CheckProductStockAsync(Guid id, int requestedQuantity);
        Task<ProductDetailDto?> GetProductDetailAsync(Guid id);
<<<<<<< HEAD
        Task<ProductDto> CreateCompleteProductAsync(CompleteProductDto completeProductDto, string createdBy);


=======
>>>>>>> parent of 8435c1b (thanh toán 2 luồng cơ bản và tạo product 6 bảng)
    }
}
