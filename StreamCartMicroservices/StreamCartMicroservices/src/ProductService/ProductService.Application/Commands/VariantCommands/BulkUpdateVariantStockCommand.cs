using MediatR;
using ProductService.Application.DTOs.Variants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.VariantCommands
{
    public class BulkUpdateVariantStockCommand : IRequest<bool>
    {
        public List<VariantStockUpdate> StockUpdates { get; set; } = new List<VariantStockUpdate>();
        public string? UpdatedBy { get; set; }
    }
}
