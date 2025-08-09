using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.DTOs
{
    public class ProductDTO
    {

        public string? Id { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string? ImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public int? QuantitySold { get; set; }
        public bool IsActive { get; set; }
        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? SKU { get; set; }
    }
}
