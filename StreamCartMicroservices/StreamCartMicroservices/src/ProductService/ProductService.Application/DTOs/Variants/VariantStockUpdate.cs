using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Variants
{
    public class VariantStockUpdate
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
