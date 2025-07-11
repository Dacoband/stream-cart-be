using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs.Images;
using ProductService.Application.Interfaces;
using Shared.Common.Models;
using Shared.Common.Services.User;
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
        private readonly IProductImageService _service;
        private readonly ICurrentUserService _currentUserService;
        public ProductImageController(IProductImageService service, ICurrentUserService currentUserService)
        {
            _service = service;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageDto>>), 200)]
        public async Task<IActionResult> GetAllImages()
        {
            var images = await _service.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ProductImageDto>>.SuccessResult(images));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetImageById(Guid id)
        {
            var image = await _service.GetByIdAsync(id);
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

            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var uploadedImage = await _service.UploadAsync(createImageDto, imageFile, userId);
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
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var updatedImage = await _service.UpdateAsync(id, updateImageDto, userId);
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
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.DeleteAsync(id, userId);
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
            var images = await _service.GetByProductIdAsync(productId);
            return Ok(ApiResponse<IEnumerable<ProductImageDto>>.SuccessResult(images));
        }

        [HttpGet("variants/{variantId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageDto>>), 200)]
        public async Task<IActionResult> GetImagesByVariantId(Guid variantId)
        {
            var images = await _service.GetByVariantIdAsync(variantId);
            return Ok(ApiResponse<IEnumerable<ProductImageDto>>.SuccessResult(images));
        }

        [HttpPatch("{id}/primary")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> SetPrimaryImage(Guid id, [FromBody] SetPrimaryImageDto setPrimaryDto)
        {
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.SetPrimaryAsync(id, setPrimaryDto.IsPrimary, userId);
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
            string? userId = _currentUserService.GetUserId().ToString();
            if (string.IsNullOrEmpty(userId))
                return BadRequest(ApiResponse<object>.ErrorResult("User ID is missing"));

            try
            {
                var result = await _service.ReorderAsync(reorderDto.ImagesOrder, userId);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Image order updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating image order: {ex.Message}"));
            }
        }
    }
}