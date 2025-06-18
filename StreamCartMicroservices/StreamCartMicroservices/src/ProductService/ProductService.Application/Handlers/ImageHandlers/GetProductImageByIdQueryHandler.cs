using MediatR;
using ProductService.Application.DTOs.Images;
using ProductService.Application.Queries.ImageQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ImageHandlers
{
    public class GetProductImageByIdQueryHandler : IRequestHandler<GetProductImageByIdQuery, ProductImageDto?>
    {
        private readonly IProductImageRepository _imageRepository;

        public GetProductImageByIdQueryHandler(IProductImageRepository imageRepository)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<ProductImageDto?> Handle(GetProductImageByIdQuery request, CancellationToken cancellationToken)
        {
            var image = await _imageRepository.GetByIdAsync(request.Id.ToString());
            if (image == null || image.IsDeleted)
            {
                return null;
            }

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