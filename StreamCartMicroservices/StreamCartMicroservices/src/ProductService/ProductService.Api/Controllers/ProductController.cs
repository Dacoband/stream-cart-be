using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;
using ProductService.Application.Interfaces;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
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

        public ProductController(IProductService productService, IShopServiceClient shopServiceClient)
        {
            _productService = productService;
            _shopServiceClient = shopServiceClient;
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
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "123";
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
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var result = await _productService.DeleteProductAsync(id, userId);

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
            [FromQuery] Guid? categoryId = null)
        {
            var pagedProducts = await _productService.GetPagedProductsAsync(pageNumber, pageSize, sortOption, activeOnly, shopId, categoryId);
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
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
    }
}