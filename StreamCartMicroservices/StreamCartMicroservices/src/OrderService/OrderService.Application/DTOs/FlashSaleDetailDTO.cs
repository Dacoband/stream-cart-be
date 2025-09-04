using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class FlashSaleDetailDTO
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public decimal FlashSalePrice { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantitySold { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Slot { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? VariantName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
