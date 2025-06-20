using Microsoft.AspNetCore.Http;
using MediatR;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Application.DTOs.Images;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.ImageQueries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IMediator _mediator;

        public ProductImageService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<ProductImageDto>> GetAllAsync()
        {
            return await _mediator.Send(new GetAllProductImagesQuery());
        }

        public async Task<ProductImageDto?> GetByIdAsync(Guid id)
        {
            return await _mediator.Send(new GetProductImageByIdQuery { Id = id });
        }

        public async Task<ProductImageDto> UploadAsync(CreateProductImageDto dto, IFormFile imageFile, string createdBy)
        {
            var command = new UploadProductImageCommand
            {
                ProductId = dto.ProductId,
                VariantId = dto.VariantId,
                Image = imageFile,
                IsPrimary = dto.IsPrimary,
                DisplayOrder = dto.DisplayOrder,
                AltText = dto.AltText,
                CreatedBy = createdBy
            };
            return await _mediator.Send(command);
        }

        public async Task<ProductImageDto> UpdateAsync(Guid id, UpdateProductImageDto dto, string updatedBy)
        {
            var command = new UpdateProductImageCommand
            {
                Id = id,
                IsPrimary = dto.IsPrimary,
                DisplayOrder = dto.DisplayOrder,
                AltText = dto.AltText,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteAsync(Guid id, string deletedBy)
        {
            var command = new DeleteProductImageCommand
            {
                Id = id,
                DeletedBy = deletedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(Guid productId)
        {
            return await _mediator.Send(new GetProductImagesByProductIdQuery { ProductId = productId });
        }

        public async Task<IEnumerable<ProductImageDto>> GetByVariantIdAsync(Guid variantId)
        {
            return await _mediator.Send(new GetProductImagesByVariantIdQuery { VariantId = variantId });
        }

        public async Task<bool> SetPrimaryAsync(Guid id, bool isPrimary, string updatedBy)
        {
            var command = new SetPrimaryImageCommand
            {
                Id = id,
                IsPrimary = isPrimary,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<bool> ReorderAsync(List<ImageOrderItem> imagesOrder, string updatedBy)
        {
            var command = new ReorderProductImagesCommand
            {
                ImagesOrder = imagesOrder,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }
    }
}