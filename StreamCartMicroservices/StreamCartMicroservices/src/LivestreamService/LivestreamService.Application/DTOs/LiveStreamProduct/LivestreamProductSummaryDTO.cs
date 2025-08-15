using System;

namespace LivestreamService.Application.DTOs
{
    public class LivestreamProductSummaryDTO
    {
        public Guid Id { get; set; }
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImageUrl { get; set; }
        //public decimal? OriginalPrice { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsPin { get; set; }
        //public bool HasFlashSale { get; set; }
        public int SoldQuantity { get; set; }
        //public int DisplayOrder { get; set; }
    }
}