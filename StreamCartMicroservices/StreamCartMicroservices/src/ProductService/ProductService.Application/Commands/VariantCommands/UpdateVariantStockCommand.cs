using MediatR;
using ProductService.Application.DTOs.Variants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.VariantCommands
{
    public class UpdateVariantStockCommand : IRequest<ProductVariantDto>
    {
        public Guid Id { get; set; }
        public int Stock { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
