using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.AttributeCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Queries.AttributeQueries;
using ProductService.Application.Queries.AttributeValueQueries;
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
        private readonly IMediator _mediator;

        public ProductAttributeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductAttributeDto>>), 200)]
        public async Task<IActionResult> GetAllAttributes()
        {
            var query = new GetAllProductAttributesQuery();
            var attributes = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<ProductAttributeDto>>.SuccessResult(attributes));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAttributeById(Guid id)
        {
            var query = new GetProductAttributeByIdQuery { Id = id };
            var attribute = await _mediator.Send(query);

            if (attribute == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Product attribute with ID {id} not found"));

            return Ok(ApiResponse<ProductAttributeDto>.SuccessResult(attribute));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductAttributeDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateAttribute([FromBody] CreateProductAttributeDto createAttributeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product attribute data"));

            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var command = new CreateProductAttributeCommand
                {
                    Name = createAttributeDto.Name,
                    CreatedBy = userId
                };

                var createdAttribute = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetAttributeById),
                    new { id = createdAttribute.Id },
                    ApiResponse<ProductAttributeDto>.SuccessResult(createdAttribute, "Product attribute created successfully")
                );
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
        public async Task<IActionResult> UpdateAttribute(Guid id, [FromBody] UpdateProductAttributeDto updateAttributeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product attribute data"));

            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

                var command = new UpdateProductAttributeCommand
                {
                    Id = id,
                    Name = updateAttributeDto.Name,
                    UpdatedBy = userId
                };

                var updatedAttribute = await _mediator.Send(command);
                return Ok(ApiResponse<ProductAttributeDto>.SuccessResult(updatedAttribute, "Product attribute updated successfully"));
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
            try
            {
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                    return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));
                var command = new DeleteProductAttributeCommand
                {
                    Id = id,
                    DeletedBy = userId
                };

                var result = await _mediator.Send(command);

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
            var query = new GetAttributesByProductIdQuery { ProductId = productId };
            var attributes = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<ProductAttributeDto>>.SuccessResult(attributes));
        }

        [HttpGet("{attributeId}/values")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttributeValueDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAttributeValues(Guid attributeId)
        {
            try
            {
                var query = new GetAttributeValuesByAttributeIdQuery { AttributeId = attributeId };
                var values = await _mediator.Send(query);

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