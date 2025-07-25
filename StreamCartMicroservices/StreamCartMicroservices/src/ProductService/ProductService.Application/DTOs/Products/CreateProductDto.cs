using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Products
{
    public class CreateProductDto
    {
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public decimal? Weight { get; set; }
       // public string? Dimensions { get; set; }
        public bool HasVariant { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public Guid? ShopId { get; set; }
    }
}
