using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Products
{
    public class UpdateProductDto
    {
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public bool? HasVariant { get; set; }
    }
}
