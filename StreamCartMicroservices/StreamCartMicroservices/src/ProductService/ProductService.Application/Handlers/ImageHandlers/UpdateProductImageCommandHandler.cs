using MediatR;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Application.DTOs.Images;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ImageHandlers
{
    public class UpdateProductImageCommandHandler : IRequestHandler<UpdateProductImageCommand, ProductImageDto>
    {
        private readonly IProductImageRepository _imageRepository;

        public UpdateProductImageCommandHandler(IProductImageRepository imageRepository)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<ProductImageDto> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
        {
            // Get the image
            var image = await _imageRepository.GetByIdAsync(request.Id.ToString());
            if (image == null)
            {
                throw new ApplicationException($"Product image with ID {request.Id} not found");
            }

            // Handle setting primary status
            if (request.IsPrimary.HasValue && request.IsPrimary.Value)
            {
                await _imageRepository.SetPrimaryImageAsync(request.Id, image.ProductId, image.VariantId);
                image.SetPrimary(true);
            }
            else if (request.IsPrimary.HasValue && !request.IsPrimary.Value)
            {
                image.SetPrimary(false);
            }

            // Update other properties
            if (request.DisplayOrder.HasValue)
            {
                image.UpdateDisplayOrder(request.DisplayOrder.Value);
            }

            if (request.AltText != null)
            {
                image.UpdateAltText(request.AltText);
            }

            // Set updater
            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                image.SetUpdatedBy(request.UpdatedBy);
            }

            // Save changes
            await _imageRepository.ReplaceAsync(image.Id.ToString(), image);

            // Return updated DTO
            return new ProductImageDto
            {
                Id = image.Id,
                ProductId = image.ProductId,
                VariantId = image.VariantId,
                ImageUrl = image.ImageUrl,
                IsPrimary = image.IsPrimary,
                DisplayOrder = image.DisplayOrder,
                AltText = image.AltText,
                CreatedAt = image.CreatedAt,
                CreatedBy = image.CreatedBy,
                LastModifiedAt = image.LastModifiedAt,
                LastModifiedBy = image.LastModifiedBy
            };
        }
    }
}