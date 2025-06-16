using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Application.DTOs.Images;
using ProductService.Application.Queries.ImageQueries;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductService.Api.Controllers
{
    [Route("api/product-images")]
    [ApiController]
    public class ProductImageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductImageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageDto>>), 200)]
        public async Task<IActionResult> GetAllImages()
        {
            var images = await _mediator.Send(new GetAllProductImagesQuery());
            return Ok(ApiResponse<IEnumerable<ProductImageDto>>.SuccessResult(images));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetImageById(Guid id)
        {
            var image = await _mediator.Send(new GetProductImageByIdQuery { Id = id });

            if (image == null)
                return NotFound(ApiResponse<object>.ErrorResult($"Product image with ID {id} not found"));

            return Ok(ApiResponse<ProductImageDto>.SuccessResult(image));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> UploadImage([FromForm] CreateProductImageDto createImageDto, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest(ApiResponse<object>.ErrorResult("No image file provided"));

            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new UploadProductImageCommand
                {
                    ProductId = createImageDto.ProductId,
                    VariantId = createImageDto.VariantId,
                    Image = imageFile,
                    IsPrimary = createImageDto.IsPrimary,
                    DisplayOrder = createImageDto.DisplayOrder,
                    AltText = createImageDto.AltText,
                    CreatedBy = userId
                };

                var uploadedImage = await _mediator.Send(command);

                return CreatedAtAction(
                    nameof(GetImageById),
                    new { id = uploadedImage.Id },
                    ApiResponse<ProductImageDto>.SuccessResult(uploadedImage, "Product image uploaded successfully")
                );
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error uploading product image: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateImage(Guid id, [FromBody] UpdateProductImageDto updateImageDto)
        {
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new UpdateProductImageCommand
                {
                    Id = id,
                    IsPrimary = updateImageDto.IsPrimary,
                    DisplayOrder = updateImageDto.DisplayOrder,
                    AltText = updateImageDto.AltText,
                    UpdatedBy = userId
                };

                var updatedImage = await _mediator.Send(command);
                return Ok(ApiResponse<ProductImageDto>.SuccessResult(updatedImage, "Product image updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating product image: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new DeleteProductImageCommand
                {
                    Id = id,
                    DeletedBy = userId
                };

                var result = await _mediator.Send(command);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResult($"Product image with ID {id} not found"));

                return Ok(ApiResponse<bool>.SuccessResult(true, "Product image deleted successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting product image: {ex.Message}"));
            }
        }

        [HttpGet("products/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageDto>>), 200)]
        public async Task<IActionResult> GetImagesByProductId(Guid productId)
        {
            var images = await _mediator.Send(new GetProductImagesByProductIdQuery { ProductId = productId });
            return Ok(ApiResponse<IEnumerable<ProductImageDto>>.SuccessResult(images));
        }

        [HttpGet("variants/{variantId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageDto>>), 200)]
        public async Task<IActionResult> GetImagesByVariantId(Guid variantId)
        {
            var images = await _mediator.Send(new GetProductImagesByVariantIdQuery { VariantId = variantId });
            return Ok(ApiResponse<IEnumerable<ProductImageDto>>.SuccessResult(images));
        }

        [HttpPatch("{id}/primary")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> SetPrimaryImage(Guid id, [FromBody] SetPrimaryImageDto setPrimaryDto)
        {
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new SetPrimaryImageCommand
                {
                    Id = id,
                    IsPrimary = setPrimaryDto.IsPrimary,
                    UpdatedBy = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Primary image status updated successfully"));
            }
            catch (ApplicationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating primary image status: {ex.Message}"));
            }
        }

        [HttpPatch("reorder")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ReorderImages([FromBody] ReorderImagesDto reorderDto)
        {
            try
            {
                string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var command = new ReorderProductImagesCommand
                {
                    ImagesOrder = reorderDto.ImagesOrder,
                    UpdatedBy = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Image order updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating image order: {ex.Message}"));
            }
        }
    }
}