using MediatR;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Application.DTOs.Images;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Services.Appwrite;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ImageHandlers
{
    public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, ProductImageDto>
    {
        private readonly IProductImageRepository _imageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IAppwriteService _appwriteService;

        public UploadProductImageCommandHandler(
            IProductImageRepository imageRepository,
            IProductRepository productRepository,
            IProductVariantRepository variantRepository,
            IAppwriteService appwriteService)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
            _appwriteService = appwriteService ?? throw new ArgumentNullException(nameof(appwriteService));
        }

        public async Task<ProductImageDto> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
        {
            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId.ToString());
            if (product == null)
            {
                throw new ApplicationException($"Product with ID {request.ProductId} not found");
            }

            // Validate variant if provided
            if (request.VariantId.HasValue)
            {
                var variant = await _variantRepository.GetByIdAsync(request.VariantId.Value.ToString());
                if (variant == null)
                {
                    throw new ApplicationException($"Variant with ID {request.VariantId} not found");
                }

                // Ensure the variant belongs to the product
                if (variant.ProductId != request.ProductId)
                {
                    throw new ApplicationException("The specified variant does not belong to the specified product");
                }
            }

            // If this is set to primary, reset other primary images
            if (request.IsPrimary)
            {
                await _imageRepository.SetPrimaryImageAsync(Guid.Empty, request.ProductId, request.VariantId);
            }

            // Upload image to Appwrite
            string imageUrl = await _appwriteService.UploadImage(request.Image);

            // Create product image entity
            var productImage = new ProductImage(
                request.ProductId,
                imageUrl,
                request.VariantId,
                request.IsPrimary,
                request.DisplayOrder,
                request.AltText,
                request.CreatedBy);

            // Save to database
            await _imageRepository.InsertAsync(productImage);

            // Return DTO
            return new ProductImageDto
            {
                Id = productImage.Id,
                ProductId = productImage.ProductId,
                VariantId = productImage.VariantId,
                ImageUrl = productImage.ImageUrl,
                IsPrimary = productImage.IsPrimary,
                DisplayOrder = productImage.DisplayOrder,
                AltText = productImage.AltText,
                CreatedAt = productImage.CreatedAt,
                CreatedBy = productImage.CreatedBy,
                LastModifiedAt = productImage.LastModifiedAt,
                LastModifiedBy = productImage.LastModifiedBy
            };
        }
    }
}