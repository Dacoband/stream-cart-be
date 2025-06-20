using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Variants
{
    public class UpdateProductVariantDto
    {
        public string? SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int Stock { get; set; }
    }
}
