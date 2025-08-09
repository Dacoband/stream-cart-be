using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.DTOs
{
    public class LivestreamProductDTO
    {
        public Guid Id { get; set; }
        public Guid LivestreamId { get; set; }
        public string? ProductId { get; set; }
        public string? VariantId { get; set; }
        //public Guid? FlashSaleId { get; set; }
        public bool IsPin { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ProductStock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string SKU { get; set; }

        // Product details from product service
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        public string? VariantName { get; set; }
    }
}
