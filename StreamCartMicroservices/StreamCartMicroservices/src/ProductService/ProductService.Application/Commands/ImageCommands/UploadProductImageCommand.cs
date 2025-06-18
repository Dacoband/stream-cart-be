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
        public IFormFile? Image { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string? AltText { get; set; }
        public string? CreatedBy { get; set; }
    }
}