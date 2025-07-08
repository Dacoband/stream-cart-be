using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs.Combinations;
using ProductService.Application.Interfaces;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductService.Api.Controllers
{
    [Route("api/product-combinations")]
    [ApiController]
    public class ProductCombinationController : ControllerBase
    {
        private readonly IProductCombinationService _service;
        private readonly ICurrentUserService _currentUserService;

        public ProductCombinationController(IProductCombinationService service, ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductCombinationDto>>), 200)]
        public async Task<IActionResult> GetAllCombinations()
        {
            var combinations = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductCombinationDto>>.SuccessResult(combinations));
        }

        [HttpGet("{variantId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductCombinationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetCombinationsByVariantId(Guid variantId)
        {
            var combinations = await _service.GetByVariantIdAsync(variantId);
            if (combinations == null || !combinations.Any())
                return NotFound(ApiResponse<object>.ErrorResult($"Combinations for variant with ID {variantId} not found"));
            return Ok(ApiResponse<IEnumerable<ProductCombinationDto>>.SuccessResult(combinations));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductCombinationDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateCombination([FromBody] CreateProductCombinationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product combination data"));

            string? userId = _currentUserService.GetUserId().ToString() ?? "123";
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var created = await _service.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetCombinationsByVariantId), new { variantId = created.VariantId },
                    ApiResponse<ProductCombinationDto>.SuccessResult(created, "Product combination created successfully"));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating product combination: {ex.Message}"));
            }
        }

        [HttpPut("{variantId}/{attributeValueId}")]
        [ProducesResponseType(typeof(ApiResponse<ProductCombinationDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateCombination(Guid variantId, Guid attributeValueId, [FromBody] UpdateProductCombinationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product combination data"));

            string? userId = _currentUserService.GetUserId().ToString() ?? "123";
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var updated = await _service.UpdateAsync(variantId, attributeValueId, dto, userId);
                return Ok(ApiResponse<ProductCombinationDto>.SuccessResult(updated, "Product combination updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product combination: {ex.Message}"));
            }
        }

        [HttpDelete("{variantId}/{attributeValueId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteCombination(Guid variantId, Guid attributeValueId)
        {
            string? userId = _currentUserService.GetUserId().ToString() ?? "123";
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.DeleteAsync(variantId, attributeValueId, userId);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult($"Combination with variant ID {variantId} and attribute value ID {attributeValueId} not found"));
                return Ok(ApiResponse<bool>.SuccessResult(true, "Product combination deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting product combination: {ex.Message}"));
            }
        }

        [HttpGet("products/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductCombinationDto>>), 200)]
        public async Task<IActionResult> GetCombinationsByProductId(Guid productId)
        {
            var combinations = await _service.GetByProductIdAsync(productId);
            return Ok(ApiResponse<IEnumerable<ProductCombinationDto>>.SuccessResult(combinations));
        }

        [HttpPost("products/{productId}/generate-combinations")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GenerateCombinations(Guid productId, [FromBody] GenerateCombinationsDto generateDto)
        {
            string? userId = _currentUserService.GetUserId().ToString() ?? "123";
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.GenerateCombinationsAsync(productId, generateDto.AttributeValueGroups, generateDto.DefaultPrice, generateDto.DefaultStock, userId);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Product combinations generated successfully"));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error generating product combinations: {ex.Message}"));
            }
        }
    }
}