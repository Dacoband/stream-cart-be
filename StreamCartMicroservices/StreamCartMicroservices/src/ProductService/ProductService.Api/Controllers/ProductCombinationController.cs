using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Application.DTOs.Combinations;
using ProductService.Application.Queries.CombinationQueries;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductService.Api.Controllers
{
    [Route("api/product-combinations")]
    [ApiController]
    public class ProductCombinationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductCombinationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductCombinationDto>>), 200)]
        public async Task<IActionResult> GetAllCombinations()
        {
            var query = new GetAllProductCombinationsQuery();
            var combinations = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<ProductCombinationDto>>.SuccessResult(combinations));
        }

        [HttpGet("{variantId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductCombinationDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetCombinationsByVariantId(Guid variantId)
        {
            var query = new GetCombinationsByVariantIdQuery { VariantId = variantId };
            var combinations = await _mediator.Send(query);

            if (combinations == null || !combinations.Any())
                return NotFound(ApiResponse<object>.ErrorResult($"Combinations for variant with ID {variantId} not found"));

            return Ok(ApiResponse<IEnumerable<ProductCombinationDto>>.SuccessResult(combinations));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductCombinationDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateCombination([FromBody] CreateProductCombinationDto createCombinationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product combination data"));

            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new CreateProductCombinationCommand
                {
                    VariantId = createCombinationDto.VariantId,
                    AttributeValueId = createCombinationDto.AttributeValueId,
                    CreatedBy = userId
                };

                var createdCombination = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetCombinationsByVariantId),
                    new { variantId = createdCombination.VariantId },
                    ApiResponse<ProductCombinationDto>.SuccessResult(createdCombination, "Product combination created successfully")
                );
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
        public async Task<IActionResult> UpdateCombination(Guid variantId, Guid attributeValueId, [FromBody] UpdateProductCombinationDto updateCombinationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid product combination data"));

            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new UpdateProductCombinationCommand
                {
                    CurrentVariantId = variantId,
                    CurrentAttributeValueId = attributeValueId,
                    NewAttributeValueId = updateCombinationDto.AttributeValueId,
                    UpdatedBy = userId
                };

                var updatedCombination = await _mediator.Send(command);
                return Ok(ApiResponse<ProductCombinationDto>.SuccessResult(updatedCombination, "Product combination updated successfully"));
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
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new DeleteProductCombinationCommand
                {
                    VariantId = variantId,
                    AttributeValueId = attributeValueId,
                    DeletedBy = userId
                };

                var result = await _mediator.Send(command);

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
            var query = new GetCombinationsByProductIdQuery { ProductId = productId };
            var combinations = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<ProductCombinationDto>>.SuccessResult(combinations));
        }

        [HttpPost("products/{productId}/generate-combinations")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GenerateCombinations(Guid productId, [FromBody] GenerateCombinationsDto generateDto)
        {
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new GenerateProductCombinationsCommand
                {
                    ProductId = productId,
                    AttributeValueGroups = generateDto.AttributeValueGroups,
                    DefaultPrice = generateDto.DefaultPrice,
                    DefaultStock = generateDto.DefaultStock,
                    CreatedBy = userId
                };

                var result = await _mediator.Send(command);
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
