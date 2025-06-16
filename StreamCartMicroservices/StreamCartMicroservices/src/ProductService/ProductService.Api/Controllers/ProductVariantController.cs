using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Variants;
using ProductService.Application.Queries.VariantQueries;
using Shared.Common.Models;
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
        private readonly IMediator _mediator;

        public ProductVariantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductVariantDto>>), 200)]
        public async Task<IActionResult> GetAllVariants()
        {
            var query = new GetAllProductVariantsQuery();
            var variants = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<ProductVariantDto>>.SuccessResult(variants));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetVariantById(Guid id)
        {
            var query = new GetProductVariantByIdQuery { Id = id };
            var variant = await _mediator.Send(query);

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

            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new CreateProductVariantCommand
                {
                    ProductId = createVariantDto.ProductId,
                    SKU = createVariantDto.SKU,
                    Price = createVariantDto.Price,
                    FlashSalePrice = createVariantDto.FlashSalePrice,
                    Stock = createVariantDto.Stock,
                    CreatedBy = userId
                };

                var createdVariant = await _mediator.Send(command);

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

            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new UpdateProductVariantCommand
                {
                    Id = id,
                    SKU = updateVariantDto.SKU,
                    Price = updateVariantDto.Price,
                    FlashSalePrice = updateVariantDto.FlashSalePrice,
                    Stock = updateVariantDto.Stock,
                    UpdatedBy = userId
                };

                var updatedVariant = await _mediator.Send(command);
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
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new DeleteProductVariantCommand
                {
                    Id = id,
                    DeletedBy = userId
                };

                var result = await _mediator.Send(command);

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
            var query = new GetVariantsByProductIdQuery { ProductId = productId };
            var variants = await _mediator.Send(query);

            return Ok(ApiResponse<IEnumerable<ProductVariantDto>>.SuccessResult(variants));
        }

        [HttpPatch("{id}/stock")]
        [ProducesResponseType(typeof(ApiResponse<ProductVariantDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateVariantStock(Guid id, [FromBody] Application.DTOs.Variants.UpdateStockDto updateStockDto)
        {
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new UpdateVariantStockCommand
                {
                    Id = id,
                    Stock = updateStockDto.Quantity,
                    UpdatedBy = userId
                };

                var updatedVariant = await _mediator.Send(command);
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
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new UpdateVariantPriceCommand
                {
                    Id = id,
                    Price = updatePriceDto.Price,
                    FlashSalePrice = updatePriceDto.FlashSalePrice,
                    UpdatedBy = userId
                };

                var updatedVariant = await _mediator.Send(command);
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
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new BulkUpdateVariantStockCommand
                {
                    StockUpdates = bulkUpdateDto.StockUpdates,
                    UpdatedBy = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Product variant stocks updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product variant stocks: {ex.Message}"));
            }
        }
    }
}