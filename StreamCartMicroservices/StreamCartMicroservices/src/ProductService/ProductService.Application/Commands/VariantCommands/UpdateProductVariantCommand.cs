using MediatR;
using ProductService.Application.DTOs.Variants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.VariantCommands
{
    public class UpdateProductVariantCommand : IRequest<ProductVariantDto>
    {
        public Guid Id { get; set; }
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int Stock { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
