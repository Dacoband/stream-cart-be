using MediatR;
using ProductService.Application.DTOs.Images;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Queries.ImageQueries
{
    public class GetAllProductImagesQuery : IRequest<IEnumerable<ProductImageDto>>
    {
    }

    public class GetProductImageByIdQuery : IRequest<ProductImageDto>
    {
        public Guid Id { get; set; }
    }

    public class GetProductImagesByProductIdQuery : IRequest<IEnumerable<ProductImageDto>>
    {
        public Guid ProductId { get; set; }
    }

    public class GetProductImagesByVariantIdQuery : IRequest<IEnumerable<ProductImageDto>>
    {
        public Guid VariantId { get; set; }
    }
}