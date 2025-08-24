using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Variants;
using ProductService.Application.Interfaces;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductService.Api.Controllers
{
    [Route("api/product-variants")]
    [ApiController]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _service;
        private readonly ICurrentUserService _currentUserService;

        public ProductVariantController(IProductVariantService service, ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductVariantDto>>), 200)]
        public async Task<IActionResult> GetAllVariants()
        {
            var variants = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductVariantDto>>.SuccessResult(variants));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetVariantById(Guid id)
        {
            var variant = await _service.GetByIdAsync(id);
            if (variant == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Product variant with ID {id} not found"));
            return Ok(ApiResponse<ProductVariantDto>.SuccessResult(variant));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateVariant([FromBody] CreateProductVariantDto createVariantDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product variant data"));

            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var createdVariant = await _service.CreateAsync(createVariantDto, userId);
                return CreatedAtAction(
                    nameof(GetVariantById),
                    new { id = createdVariant.Id },
                    ApiResponse<ProductVariantDto>.SuccessResult(createdVariant, "Product variant created successfully")
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating product variant: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateVariant(Guid id, [FromBody] UpdateProductVariantDto updateVariantDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product variant data"));

            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var updatedVariant = await _service.UpdateAsync(id, updateVariantDto, userId);
                return Ok(ApiResponse<ProductVariantDto>.SuccessResult(updatedVariant, "Product variant updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product variant: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteVariant(Guid id)
        {
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.DeleteAsync(id, userId);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product variant with ID {id} not found"));
                return Ok(ApiResponse<bool>.SuccessResult(true, "Product variant deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting product variant: {ex.Message}"));
            }
        }

        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductVariantDto>>), 200)]
        public async Task<IActionResult> GetVariantsByProductId(Guid productId)
        {
            var variants = await _service.GetByProductIdAsync(productId);
            return Ok(ApiResponse<IEnumerable<ProductVariantDto>>.SuccessResult(variants));
        }

        [HttpPatch("{id}/stock")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateVariantStock(Guid id, [FromBody] Application.DTOs.Variants.UpdateStockDto updateStockDto)
        {
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var updatedVariant = await _service.UpdateStockAsync(id, updateStockDto.Quantity, userId);
                return Ok(ApiResponse<ProductVariantDto>.SuccessResult(updatedVariant, "Product variant stock updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product variant stock: {ex.Message}"));
            }
        }

        [HttpPatch("{id}/price")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateVariantPrice(Guid id, [FromBody] UpdatePriceDto updatePriceDto)
        {
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var updatedVariant = await _service.UpdatePriceAsync(id, updatePriceDto.Price, updatePriceDto.FlashSalePrice, userId);
                return Ok(ApiResponse<ProductVariantDto>.SuccessResult(updatedVariant, "Product variant price updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product variant price: {ex.Message}"));
            }
        }

        [HttpPost("bulk-update-stock")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> BulkUpdateStock([FromBody] BulkUpdateStockDto bulkUpdateDto)
        {
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.BulkUpdateStockAsync((IEnumerable<BulkUpdateStockDto>)bulkUpdateDto.StockUpdates, userId);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Product variant stocks updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product variant stocks: {ex.Message}"));
            }
        }
        [HttpGet("{id}/dimensions")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantDto1>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetVariantWithDimensions(Guid id)
        {
            try
            {
                // You'll need to implement this in your service or use the existing GetByIdAsync
                var variant = await _service.GetByIdAsync1(id);
                if (variant == null)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product variant with ID {id} not found"));
                return Ok(ApiResponse<ProductVariantDto1>.SuccessResult(variant));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error getting product variant with dimensions: {ex.Message}"));
            }
        }

    }
}