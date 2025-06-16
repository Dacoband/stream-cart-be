using MediatR;
using ProductService.Application.DTOs.Images;
using ProductService.Application.Queries.ImageQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ImageHandlers
{
    public class GetProductImagesByVariantIdQueryHandler : IRequestHandler<GetProductImagesByVariantIdQuery, IEnumerable<ProductImageDto>>
    {
        private readonly IProductImageRepository _imageRepository;

        public GetProductImagesByVariantIdQueryHandler(IProductImageRepository imageRepository)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<IEnumerable<ProductImageDto>> Handle(GetProductImagesByVariantIdQuery request, CancellationToken cancellationToken)
        {
            var images = await _imageRepository.GetByVariantIdAsync(request.VariantId);

            var activeImages = images.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder);

            return activeImages.Select(i => new ProductImageDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                VariantId = i.VariantId,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder,
                AltText = i.AltText,
                CreatedAt = i.CreatedAt,
                CreatedBy = i.CreatedBy,
                LastModifiedAt = i.LastModifiedAt,
                LastModifiedBy = i.LastModifiedBy
            });
        }
    }
}