using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.ProductCommands;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.ProductQueries;
using ProductService.Domain.Enums;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductService.Api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _logger;
        private readonly IProductRepository _productRepository;

        public ProductController(IProductService productService, IShopServiceClient shopServiceClient, ICurrentUserService currentUserService, IMediator mediator, ILogger<ProductController> logger, IProductRepository productRepository)
        {
            _productService = productService;
            _shopServiceClient = shopServiceClient;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _logger = logger;
            _productRepository = productRepository;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
                return BadRequest(ApiResponse<object>.ErrorResult("Product data is missing"));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product data"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString() ?? "123"; 
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var createdProduct = await _productService.CreateProductAsync(createProductDto, userId);

                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = createdProduct.Id },
                    ApiResponse<ProductDto>.SuccessResult(createdProduct, "Product created successfully")
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating product: {ex.Message}"));
            }
        }
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> DoesProductExist(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                var exists = product != null;
                return Ok(ApiResponse<bool>.SuccessResult(exists,
                    exists ? "Product exists" : "Product not found"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error checking product existence: {ex.Message}"));
            }
        }

        
        [HttpGet("{productId}/shop/{shopId}/owned")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> IsProductOwnedByShop(Guid productId, Guid shopId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);

                if (product == null)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(false, "Product not found"));
                }

                var isOwned = product.ShopId == shopId;
                return Ok(ApiResponse<bool>.SuccessResult(isOwned,
                    isOwned ? "Product belongs to the shop" : "Product does not belong to the shop"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error checking product ownership: {ex.Message}"));
            }
        }
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product data"));

            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var updatedProduct = await _productService.UpdateProductAsync(id, updateProductDto, userId);
                return Ok(ApiResponse<ProductDto>.SuccessResult(updatedProduct, "Product updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteProduct(Guid id, [FromBody]string? reason)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var result = await _productService.DeleteProductAsync(id, userId.ToString(), reason);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product with ID {id} not found"));

                return Ok(ApiResponse<bool>.SuccessResult(true, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting product: {ex.Message}"));
            }
        }
        [HttpPatch("activate/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> ActivateProduct(Guid id)
        {
            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var result = await _productService.ActivateProductAsync(id, userId);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product with ID {id} not found"));

                return Ok(ApiResponse<bool>.SuccessResult(true, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting product: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Product with ID {id} not found"));

            return Ok(ApiResponse<ProductDto>.SuccessResult(product));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetAllProducts([FromQuery] bool activeOnly = false)
        {
            var products = await _productService.GetAllProductsAsync(activeOnly);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), 200)]
        public async Task<IActionResult> GetPagedProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ProductSortOption sortOption = ProductSortOption.DateCreatedDesc,
            [FromQuery] bool activeOnly = false,
            [FromQuery] Guid? shopId = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] bool? InStockOnly = false)
        {
            var pagedProducts = await _productService.GetPagedProductsAsync(pageNumber, pageSize, sortOption, activeOnly, shopId, categoryId, InStockOnly);
            return Ok(ApiResponse<PagedResult<ProductDto>>.SuccessResult(pagedProducts));
        }

        [HttpGet("shop/{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetProductsByShopId(Guid shopId, [FromQuery] bool activeOnly = false)
        {
            var shopExists = await _shopServiceClient.DoesShopExistAsync(shopId);
            if (!shopExists)
                return NotFound($"Shop with ID {shopId} not found");
            var products = await _productService.GetProductsByShopIdAsync(shopId, activeOnly);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
        }

        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetProductsByCategoryId(Guid categoryId, [FromQuery] bool activeOnly = false)
        {
            var products = await _productService.GetProductsByCategoryIdAsync(categoryId, activeOnly);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
        }

        [HttpGet("bestselling")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetBestSellingProducts(
            [FromQuery] int count = 10,
            [FromQuery] Guid? shopId = null,
            [FromQuery] Guid? categoryId = null)
        {
            var products = await _productService.GetBestSellingProductsAsync(count, shopId, categoryId);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateProductStatus(Guid id, [FromBody] bool isActive)
        {
            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var updatedProduct = await _productService.UpdateProductStatusAsync(id, isActive, userId);

                string statusMessage = isActive ? "activated" : "deactivated";
                return Ok(ApiResponse<ProductDto>.SuccessResult(updatedProduct, $"Product {statusMessage} successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product status: {ex.Message}"));
            }
        }

        [HttpPatch("{id}/stock")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateProductStock(Guid id, [FromBody] UpdateStockDto updateStockDto)
        {
            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var updatedProduct = await _productService.UpdateProductStockAsync(id, updateStockDto.Quantity, userId);
                return Ok(ApiResponse<ProductDto>.SuccessResult(updatedProduct, "Product stock updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product stock: {ex.Message}"));
            }
        }

        [HttpPost("{id}/check-stock")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> CheckProductStock(Guid id, [FromBody] CheckStockDto checkStockDto)
        {
            try
            {
                var hasStock = await _productService.CheckProductStockAsync(id, checkStockDto.RequestedQuantity);

                if (hasStock)
                {
                    return Ok(ApiResponse<bool>.SuccessResult(true, "Product has sufficient stock"));
                }
                else
                {
                    return Ok(ApiResponse<bool>.SuccessResult(false, "Insufficient stock for this product"));
                }
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error checking product stock: {ex.Message}"));
            }
        }

        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetProductDetail(Guid id)
        {
            try
            {
                var productDetail = await _productService.GetProductDetailAsync(id);

                if (productDetail == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product with ID {id} not found"));

                return Ok(ApiResponse<ProductDetailDto>.SuccessResult(productDetail));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving product details: {ex.Message}"));
            }
        }
        [HttpPost("complete")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateCompleteProduct([FromBody] CompleteProductDto completeProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdBy = User.Identity?.Name ?? "Anonymous";
                var result = await _productService.CreateCompleteProductAsync(completeProductDto, createdBy);
                return CreatedAtAction(nameof(GetProductById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("flash-sales")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetProductsWithFlashSale()
        {
            try
            {
                var query = new GetProductsWithFlashSaleQuery();
                var response = await _mediator.Send(query);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi khi lấy danh sách sản phẩm có Flash Sale: {ex.Message}"));
            }
        }
        /// </summary>
        /// <param name="request">Thông tin tìm kiếm và bộ lọc</param>
        /// <returns>Danh sách sản phẩm phù hợp</returns>
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<SearchProductResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> SearchProducts([FromQuery] SearchProductRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu tìm kiếm không hợp lệ"));
            }

            try
            {
                var query = new SearchProductsQuery
                {
                    SearchTerm = request.SearchTerm,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    CategoryId = request.CategoryId,
                    MinPrice = request.MinPrice,
                    MaxPrice = request.MaxPrice,
                    ShopId = request.ShopId,
                    SortBy = request.SortBy,
                    InStockOnly = request.InStockOnly,
                    MinRating = request.MinRating,
                    OnSaleOnly = request.OnSaleOnly
                };

                var result = await _mediator.Send(query);

                return Ok(ApiResponse<SearchProductResponseDto>.SuccessResult(
                    result,
                    $"Tìm thấy {result.TotalResults} sản phẩm cho '{request.SearchTerm}' trong {result.SearchTimeMs:F2}ms"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm sản phẩm với từ khóa: {SearchTerm}", request.SearchTerm);
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi tìm kiếm sản phẩm"));
            }
        }
        /// <summary>
        /// Lấy gợi ý tìm kiếm nhanh
        /// </summary>
        /// <param name="q">Từ khóa cần gợi ý</param>
        /// <returns>Danh sách gợi ý</returns>
        [HttpGet("search/suggestions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string q)
        {
            try
            {
                // Simple suggestion logic - could be enhanced with Redis cache or Elasticsearch
                var suggestions = new List<string>();

                if (!string.IsNullOrWhiteSpace(q) && q.Length >= 2)
                {
                    // Mock suggestions - in reality, this would query popular search terms
                    suggestions.AddRange(new[]
                    {
                        $"{q} áo",
                        $"{q} quần",
                        $"{q} giày",
                        $"{q} túi xách",
                        $"{q} phụ kiện"
                    }.Take(5));
                }

                return Ok(ApiResponse<List<string>>.SuccessResult(suggestions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý tìm kiếm cho: {Query}", q);
                return Ok(ApiResponse<List<string>>.SuccessResult(new List<string>()));
            }
        }
        /// <summary>
        /// Lấy sản phẩm phổ biến/trending
        /// </summary>
        [HttpGet("trending")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<ProductSearchItemDto>>), 200)]
        public async Task<IActionResult> GetTrendingProducts([FromQuery] int limit = 10)
        {
            try
            {
                var query = new SearchProductsQuery
                {
                    SearchTerm = "",
                    PageNumber = 1,
                    PageSize = limit,
                    SortBy = "best_selling"
                };

                var result = await _mediator.Send(query);

                return Ok(ApiResponse<List<ProductSearchItemDto>>.SuccessResult(
                    result.Products.Items.ToList(),
                    "Lấy danh sách sản phẩm trending thành công"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm trending");
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra"));
            }
        }
        [HttpGet("shop/{shopId}/count")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        public async Task<IActionResult> GetProductCountByShopId(Guid shopId, [FromQuery] bool activeOnly = true)
        {
            try
            {
                var shopExists = await _shopServiceClient.DoesShopExistAsync(shopId);
                if (!shopExists)
                    return NotFound(ApiResponse<object>.ErrorResult($"Shop with ID {shopId} not found"));

                var products = await _productService.GetProductsByShopIdAsync(shopId, activeOnly);
                var count = products?.Count() ?? 0;

                return Ok(ApiResponse<int>.SuccessResult(count,
                    $"Found {count} {(activeOnly ? "active " : "")}products for shop {shopId}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error counting products: {ex.Message}"));
            }
        }
        // ✅ BỔ SUNG: API cập nhật stock sản phẩm với quantity change
        [HttpPut("{id}/stock")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateProductStockQuantityChange(Guid id, [FromBody] UpdateStockQuantityChangeDto updateStockDto)
        {
            try
            {
                string? userId = _currentUserService.GetUserId().ToString();
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                // Lấy sản phẩm hiện tại
                var currentProduct = await _productService.GetProductByIdAsync(id);
                if (currentProduct == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product with ID {id} not found"));

                // Tính toán số lượng mới
                var newQuantity = Math.Max(0, currentProduct.StockQuantity + updateStockDto.QuantityChange);

                var updatedProduct = await _productService.UpdateProductStockAsync(id, newQuantity, userId);
                return Ok(ApiResponse<ProductDto>.SuccessResult(updatedProduct,
                    $"Product stock updated by {updateStockDto.QuantityChange}. New stock: {newQuantity}"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product stock: {ex.Message}"));
            }
        }
        [HttpGet("shop/{shopId}/search")]
        [ProducesResponseType(typeof(ApiResponse<SearchProductResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> SearchProductsInShop(
    Guid shopId,
    [FromQuery] SearchProductInShopRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu tìm kiếm không hợp lệ"));
            }

            try
            {
                // Kiểm tra shop có tồn tại không
                var shopExists = await _shopServiceClient.DoesShopExistAsync(shopId);
                if (!shopExists)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Shop với ID {shopId} không tồn tại"));
                }

                // Tạo query tìm kiếm với filter theo shop
                var query = new SearchProductsQuery
                {
                    SearchTerm = request.SearchTerm ?? "",
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    ShopId = shopId, // Lọc theo shop cụ thể
                    CategoryId = request.CategoryId,
                    MinPrice = request.MinPrice,
                    MaxPrice = request.MaxPrice,
                    SortBy = request.SortBy,
                    InStockOnly = request.InStockOnly,
                    MinRating = request.MinRating,
                    OnSaleOnly = request.OnSaleOnly
                };

                var result = await _mediator.Send(query);

                return Ok(ApiResponse<SearchProductResponseDto>.SuccessResult(
                    result,
                    $"Tìm thấy {result.TotalResults} sản phẩm cho '{request.SearchTerm}' trong shop trong {result.SearchTimeMs:F2}ms"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm sản phẩm trong shop {ShopId} với từ khóa: {SearchTerm}", shopId, request.SearchTerm);
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi tìm kiếm sản phẩm trong shop"));
            }
        }

        /// <summary>
        /// Lấy sản phẩm phổ biến trong shop
        /// </summary>
        [HttpGet("shop/{shopId}/trending")]
        [ProducesResponseType(typeof(ApiResponse<List<ProductSearchItemDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetTrendingProductsInShop(Guid shopId, [FromQuery] int limit = 10)
        {
            try
            {
                // Kiểm tra shop có tồn tại không
                var shopExists = await _shopServiceClient.DoesShopExistAsync(shopId);
                if (!shopExists)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Shop với ID {shopId} không tồn tại"));
                }

                var query = new SearchProductsQuery
                {
                    SearchTerm = "",
                    PageNumber = 1,
                    PageSize = limit,
                    ShopId = shopId,
                    SortBy = "best_selling"
                };

                var result = await _mediator.Send(query);

                return Ok(ApiResponse<List<ProductSearchItemDto>>.SuccessResult(
                    result.Products.Items.ToList(),
                    $"Lấy danh sách {result.Products.Items.Count()} sản phẩm trending của shop thành công"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm trending của shop {ShopId}", shopId);
                return BadRequest(ApiResponse<object>.ErrorResult("Có lỗi xảy ra khi lấy sản phẩm trending"));
            }
        }

        /// <summary>
        /// Lấy gợi ý tìm kiếm trong shop
        /// </summary>
        [HttpGet("shop/{shopId}/search/suggestions")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetSearchSuggestionsInShop(Guid shopId, [FromQuery] string q)
        {
            try
            {
                // Kiểm tra shop có tồn tại không
                var shopExists = await _shopServiceClient.DoesShopExistAsync(shopId);
                if (!shopExists)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Shop với ID {shopId} không tồn tại"));
                }

                var suggestions = new List<string>();

                if (!string.IsNullOrWhiteSpace(q) && q.Length >= 2)
                {
                    // Có thể tích hợp với cache hoặc database để lấy gợi ý thực tế
                    // Hiện tại tạo gợi ý đơn giản
                    suggestions.AddRange(new[]
                    {
                $"{q}",
                $"{q} sale",
                $"{q} giảm giá",
                $"{q} mới",
                $"{q} hot"
            }.Take(5));
                }

                return Ok(ApiResponse<List<string>>.SuccessResult(suggestions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy gợi ý tìm kiếm cho shop {ShopId}: {Query}", shopId, q);
                return Ok(ApiResponse<List<string>>.SuccessResult(new List<string>()));
            }
        }
        [HttpPut("{id}/quantity-sold")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateProductQuantitySold(Guid id, [FromBody] UpdateProductQuantitySoldDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));

            try
            {
                var command = new UpdateProductQuantitySoldCommand
                {
                    ProductId = id,
                    QuantityChange = request.QuantityChange,
                    UpdatedBy = request.UpdatedBy ?? "OrderService"
                };

                var result = await _mediator.Send(command);

                if (!result)
                {
                    return NotFound(ApiResponse<object>.ErrorResult("Không tìm thấy sản phẩm"));
                }

                return Ok(ApiResponse<bool>.SuccessResult(true, "Cập nhật số lượng đã bán thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity sold for product {ProductId}", id);
                return BadRequest(ApiResponse<object>.ErrorResult("Lỗi khi cập nhật số lượng đã bán"));
            }
        }

        public class UpdateQuantitySoldRequest
        {
            [Required]
            public int QuantityChange { get; set; }
            public string? UpdatedBy { get; set; }
        }
    }
}