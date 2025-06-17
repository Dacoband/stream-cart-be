using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.AttributeValueCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Queries.AttributeValueQueries;
using Shared.Common.Models;
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
        private readonly IMediator _mediator;

        public AttributeValueController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttributeValueDto>>), 200)]
        public async Task<IActionResult> GetAllAttributeValues()
        {
            var query = new GetAllAttributeValuesQuery();
            var attributeValues = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<AttributeValueDto>>.SuccessResult(attributeValues));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AttributeValueDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAttributeValueById(Guid id)
        {
            var query = new GetAttributeValueByIdQuery { Id = id };
            var attributeValue = await _mediator.Send(query);

            if (attributeValue == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Attribute value with ID {id} not found"));

            return Ok(ApiResponse<AttributeValueDto>.SuccessResult(attributeValue));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AttributeValueDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateAttributeValue([FromBody] CreateAttributeValueDto createAttributeValueDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid attribute value data"));

            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var command = new CreateAttributeValueCommand
                {
                    AttributeId = createAttributeValueDto.AttributeId,
                    ValueName = createAttributeValueDto.ValueName,
                    CreatedBy = userId
                };

                var createdAttributeValue = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetAttributeValueById),
                    new { id = createdAttributeValue.Id },
                    ApiResponse<AttributeValueDto>.SuccessResult(createdAttributeValue, "Attribute value created successfully")
                );
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
        public async Task<IActionResult> UpdateAttributeValue(Guid id, [FromBody] UpdateAttributeValueDto updateAttributeValueDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid attribute value data"));

            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var command = new UpdateAttributeValueCommand
                {
                    Id = id,
                    ValueName = updateAttributeValueDto.ValueName,
                    UpdatedBy = userId
                };

                var updatedAttributeValue = await _mediator.Send(command);
                return Ok(ApiResponse<AttributeValueDto>.SuccessResult(updatedAttributeValue, "Attribute value updated successfully"));
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
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var command = new DeleteAttributeValueCommand
                {
                    Id = id,
                    DeletedBy = userId
                };

                var result = await _mediator.Send(command);

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
                var query = new GetAttributeValuesByAttributeIdQuery { AttributeId = attributeId };
                var attributeValues = await _mediator.Send(query);

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