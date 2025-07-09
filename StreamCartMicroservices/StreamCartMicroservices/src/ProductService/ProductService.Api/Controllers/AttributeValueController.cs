using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Interfaces;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductService.Api.Controllers
{
    [Route("api/attribute-values")]
    [ApiController]
    public class AttributeValueController : ControllerBase
    {
        private readonly IAttributeValueService _service;
        private readonly ICurrentUserService _currentUserService;

        public AttributeValueController(IAttributeValueService service, ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttributeValueDto>>), 200)]
        public async Task<IActionResult> GetAllAttributeValues()
        {
            var attributeValues = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<AttributeValueDto>>.SuccessResult(attributeValues));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AttributeValueDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAttributeValueById(Guid id)
        {
            var attributeValue = await _service.GetByIdAsync(id);
            if (attributeValue == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Attribute value with ID {id} not found"));
            return Ok(ApiResponse<AttributeValueDto>.SuccessResult(attributeValue));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AttributeValueDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateAttributeValue([FromBody] CreateAttributeValueDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid attribute value data"));

            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));
            if (dto.ValueName == null)
                return BadRequest(ApiResponse<object>.ErrorResult("ValueName cannot be null"));

            try
            {
                var created = await _service.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetAttributeValueById), new { id = created.Id },
                    ApiResponse<AttributeValueDto>.SuccessResult(created, "Attribute value created successfully"));
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating attribute value: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AttributeValueDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateAttributeValue(Guid id, [FromBody] UpdateAttributeValueDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid attribute value data"));

            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var updated = await _service.UpdateAsync(id, dto, userId);
                return Ok(ApiResponse<AttributeValueDto>.SuccessResult(updated, "Attribute value updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating attribute value: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> DeleteAttributeValue(Guid id)
        {
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.DeleteAsync(id, userId);
                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult($"Attribute value with ID {id} not found"));
                return Ok(ApiResponse<bool>.SuccessResult(true, "Attribute value deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting attribute value: {ex.Message}"));
            }
        }

        [HttpGet("attribute/{attributeId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttributeValueDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetValuesByAttributeId(Guid attributeId)
        {
            try
            {
                var attributeValues = await _service.GetByAttributeIdAsync(attributeId);
                return Ok(ApiResponse<IEnumerable<AttributeValueDto>>.SuccessResult(attributeValues));
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