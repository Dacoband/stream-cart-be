using System;
using System.Collections.Generic;

namespace LivestreamService.Application.DTOs
{
    /// <summary>
    /// DTO representing a product in livestream with all its variants
    /// </summary>
    public class ProductLiveStreamDTO
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public bool HasVariants { get; set; }
        public int TotalActualStock { get; set; }
        public List<LivestreamProductVariantDTO> Variants { get; set; } = new List<LivestreamProductVariantDTO>();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    /// <summary>
    /// DTO representing a product variant in livestream
    /// </summary>
    public class LivestreamProductVariantDTO
    {
        public Guid Id { get; set; }
        public Guid LivestreamId { get; set; }
        public string? ProductId { get; set; }
        public string? VariantId { get; set; }
        public string? VariantName { get; set; }
        public string? SKU { get; set; }
        public bool IsPin { get; set; }
        public decimal Price { get; set; }
        public int LivestreamStock { get; set; } // Stock set for this livestream
        public int ActualStock { get; set; } // Actual stock in inventory
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}