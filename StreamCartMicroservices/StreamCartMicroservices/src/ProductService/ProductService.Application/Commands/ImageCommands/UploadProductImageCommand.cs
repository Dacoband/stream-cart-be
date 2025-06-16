using MediatR;
using Microsoft.AspNetCore.Http;
using ProductService.Application.DTOs.Images;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Commands.ImageCommands
{
    public class UploadProductImageCommand : IRequest<ProductImageDto>
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public IFormFile Image { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string AltText { get; set; }
        public string CreatedBy { get; set; }
    }

    public class UpdateProductImageCommand : IRequest<ProductImageDto>
    {
        public Guid Id { get; set; }
        public bool? IsPrimary { get; set; }
        public int? DisplayOrder { get; set; }
        public string AltText { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class DeleteProductImageCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string DeletedBy { get; set; }
    }

    public class SetPrimaryImageCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public bool IsPrimary { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class ReorderProductImagesCommand : IRequest<bool>
    {
        public List<ImageOrderItem> ImagesOrder { get; set; }
        public string UpdatedBy { get; set; }
    }
}