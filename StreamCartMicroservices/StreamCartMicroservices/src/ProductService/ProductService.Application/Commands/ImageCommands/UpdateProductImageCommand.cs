using MediatR;
using ProductService.Application.DTOs.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.ImageCommands
{
    public class UpdateProductImageCommand : IRequest<ProductImageDto>
    {
        public Guid Id { get; set; }
        public bool? IsPrimary { get; set; }
        public int? DisplayOrder { get; set; }
        public string? AltText { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
