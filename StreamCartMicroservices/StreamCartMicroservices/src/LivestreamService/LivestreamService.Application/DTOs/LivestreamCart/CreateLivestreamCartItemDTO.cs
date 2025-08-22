using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Application.DTOs.LivestreamCart
{
    public class CreateLivestreamCartItemDTO
    {
        [Required]
        public Guid LivestreamId { get; set; }

        [Required]
        public Guid LivestreamProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;
    }

    public class LivestreamCartResponseDTO
    {
        public Guid LivestreamCartId { get; set; }
        public Guid LivestreamId { get; set; }
        public Guid ViewerId { get; set; }
        public List<LivestreamCartItemDTO> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal SubTotal { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LivestreamCartItemDTO
    {
        public Guid Id { get; set; }
        public Guid LivestreamProductId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string? VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public decimal LivestreamPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountPercentage => OriginalPrice > 0 ?
            Math.Round((OriginalPrice - LivestreamPrice) / OriginalPrice * 100, 2) : 0;
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public string PrimaryImage { get; set; } = string.Empty;
        public Dictionary<string, string>? Attributes { get; set; }
        public bool ProductStatus { get; set; }
        public decimal TotalPrice => LivestreamPrice * Quantity;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateLivestreamCartItemDTO
    {
        [Required]
        public Guid LivestreamCartItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0")]
        public int Quantity { get; set; }
    }

    public class LivestreamCartSummaryDTO
    {
        public int TotalProducts { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public bool HasExpiredItems { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}