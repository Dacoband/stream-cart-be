using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductService.Api.Controllers
{
    [Route("api/product-attributes")]
    [ApiController]
    public class ProductAttributeController : ControllerBase
    {
        private readonly IProductAttributeService _service;

        public ProductAttributeController(IProductAttributeService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductAttributeDto>>), 200)]
        public async Task<IActionResult> GetAllAttributes()
        {
            var attributes = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductAttributeDto>>.SuccessResult(attributes));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAttributeById(Guid id)
        {
            var attribute = await _service.GetByIdAsync(id);
            if (attribute == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Product attribute with ID {id} not found"));
            return Ok(ApiResponse<ProductAttributeDto>.SuccessResult(attribute));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateAttribute([FromBody] CreateProductAttributeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product attribute data"));

            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var created = await _service.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetAttributeById), new { id = created.Id },
                    ApiResponse<ProductAttributeDto>.SuccessResult(created, "Product attribute created successfully"));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating product attribute: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateAttribute(Guid id, [FromBody] UpdateProductAttributeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product attribute data"));

            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var updated = await _service.UpdateAsync(id, dto, userId);
                return Ok(ApiResponse<ProductAttributeDto>.SuccessResult(updated, "Product attribute updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product attribute: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> DeleteAttribute(Guid id)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.DeleteAsync(id, userId);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product attribute with ID {id} not found"));
                return Ok(ApiResponse<bool>.SuccessResult(true, "Product attribute deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting product attribute: {ex.Message}"));
            }
        }

        [HttpGet("products/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductAttributeDto>>), 200)]
        public async Task<IActionResult> GetAttributesByProductId(Guid productId)
        {
            var attributes = await _service.GetByProductIdAsync(productId);
            return Ok(ApiResponse<IEnumerable<ProductAttributeDto>>.SuccessResult(attributes));
        }

        [HttpGet("{attributeId}/values")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttributeValueDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAttributeValues(Guid attributeId)
        {
            try
            {
                var values = await _service.GetValuesByAttributeIdAsync(attributeId);
                return Ok(ApiResponse<IEnumerable<AttributeValueDto>>.SuccessResult(values));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving attribute values: {ex.Message}"));
            }
        }
    }
}