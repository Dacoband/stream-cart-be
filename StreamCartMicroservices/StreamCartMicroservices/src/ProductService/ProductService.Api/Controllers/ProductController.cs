using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Details;
using ProductService.Application.Queries;
using ProductService.Application.Queries.DetailQueries;
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
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        //[Authorize] // Thường sẽ cần role Seller hoặc Admin
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product data"));

            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var command = new CreateProductCommand
                {
                    ProductName = createProductDto.ProductName,
                    Description = createProductDto.Description,
                    SKU = createProductDto.SKU,
                    CategoryId = createProductDto.CategoryId,
                    BasePrice = createProductDto.BasePrice,
                    DiscountPrice = createProductDto.DiscountPrice,
                    StockQuantity = createProductDto.StockQuantity,
                    Weight = createProductDto.Weight,
                    Dimensions = createProductDto.Dimensions,
                    HasVariant = createProductDto.HasVariant,
                    ShopId = createProductDto.ShopId,
                    CreatedBy = userId
                };

                var createdProduct = await _mediator.Send(command);

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
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product data"));

            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));
                var command = new UpdateProductCommand
                {
                    Id = id,
                    ProductName = updateProductDto.ProductName,
                    Description = updateProductDto.Description,
                    SKU = updateProductDto.SKU,
                    CategoryId = updateProductDto.CategoryId,
                    BasePrice = updateProductDto.BasePrice,
                    DiscountPrice = updateProductDto.DiscountPrice,
                    Weight = updateProductDto.Weight,
                    Dimensions = updateProductDto.Dimensions,
                    HasVariant = updateProductDto.HasVariant,
                    UpdatedBy = userId
                };

                var updatedProduct = await _mediator.Send(command);
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
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));
                var command = new DeleteProductCommand
                {
                    Id = id,
                    DeletedBy = userId
                };

                var result = await _mediator.Send(command);

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
            var query = new GetProductByIdQuery { Id = id };
            var product = await _mediator.Send(query);

            if (product == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Product with ID {id} not found"));

            return Ok(ApiResponse<ProductDto>.SuccessResult(product));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetAllProducts([FromQuery] bool activeOnly = false)
        {
            var query = new GetAllProductsQuery { ActiveOnly = activeOnly };
            var products = await _mediator.Send(query);

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
            var query = new GetPagedProductsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortOption = sortOption,
                ActiveOnly = activeOnly,
                ShopId = shopId,
                CategoryId = categoryId
            };

            var pagedProducts = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<ProductDto>>.SuccessResult(pagedProducts));
        }

        [HttpGet("shop/{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetProductsByShopId(Guid shopId, [FromQuery] bool activeOnly = false)
        {
            var query = new GetProductsByShopIdQuery
            {
                ShopId = shopId,
                ActiveOnly = activeOnly
            };

            var products = await _mediator.Send(query);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
        }

        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetProductsByCategoryId(Guid categoryId, [FromQuery] bool activeOnly = false)
        {
            var query = new GetProductsByCategoryIdQuery
            {
                CategoryId = categoryId,
                ActiveOnly = activeOnly
            };

            var products = await _mediator.Send(query);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
        }

        [HttpGet("bestselling")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
        public async Task<IActionResult> GetBestSellingProducts(
            [FromQuery] int count = 10,
            [FromQuery] Guid? shopId = null,
            [FromQuery] Guid? categoryId = null)
        {
            var query = new GetBestSellingProductsQuery
            {
                Count = count,
                ShopId = shopId,
                CategoryId = categoryId
            };

            var products = await _mediator.Send(query);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResult(products));
        }

        [HttpPatch("{id}/status")]
       // [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateProductStatus(Guid id, [FromBody] bool isActive)
        {
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));
                var command = new UpdateProductStatusCommand
                {
                    Id = id,
                    IsActive = isActive,
                    UpdatedBy = userId
                };

                var updatedProduct = await _mediator.Send(command);

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
       // [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateProductStock(Guid id, [FromBody] UpdateStockDto updateStockDto)
        {
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));
                var command = new UpdateProductStockCommand
                {
                    Id = id,
                    StockQuantity = updateStockDto.Quantity,
                    UpdatedBy = userId
                };

                var updatedProduct = await _mediator.Send(command);
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
                var command = new CheckProductStockCommand
                {
                    ProductId = id,
                    RequestedQuantity = checkStockDto.RequestedQuantity
                };

                var hasStock = await _mediator.Send(command);

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
                var query = new GetProductDetailQuery { ProductId = id };
                var productDetail = await _mediator.Send(query);

                if (productDetail == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product with ID {id} not found"));

                return Ok(ApiResponse<ProductDetailDto>.SuccessResult(productDetail));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving product details: {ex.Message}"));
            }
        }
    }
}
