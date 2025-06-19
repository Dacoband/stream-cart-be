using MediatR;
using ProductService.Application.DTOs.Variants;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Commands.VariantCommands
{
    public class CreateProductVariantCommand : IRequest<ProductVariantDto>
    {
        public Guid ProductId { get; set; }
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int Stock { get; set; }
        public string? CreatedBy { get; set; }
    }
}