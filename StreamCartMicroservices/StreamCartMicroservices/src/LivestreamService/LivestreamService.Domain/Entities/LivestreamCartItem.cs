using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LivestreamService.Domain.Entities
{
    public class LivestreamCartItem : BaseEntity
    {
        [Required]
        [ForeignKey("LivestreamCart")]
        public Guid LivestreamCartId { get; set; }

        [Required]
        public Guid LivestreamId { get; set; }

        [Required]
        public Guid LivestreamProductId { get; set; } // Reference to LivestreamProduct

        [Required]
        public string ProductId { get; set; } = string.Empty;

        public string? VariantId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public Guid ShopId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ShopName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LivestreamPrice { get; set; } // Giá trong livestream (có thể có discount)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; } // Giá gốc

        [Required]
        public int Stock { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(500)]
        public string PrimaryImage { get; set; } = string.Empty;

        public bool ProductStatus { get; set; } = true;

        [Column(TypeName = "jsonb")]
        public Dictionary<string, string>? Attributes { get; set; } = new();

        // Navigation properties
        public virtual LivestreamCart? LivestreamCart { get; set; }
        public virtual LivestreamProduct? LivestreamProduct { get; set; }

        private LivestreamCartItem() { }

        public LivestreamCartItem(
            Guid livestreamCartId,
            Guid livestreamId,
            Guid livestreamProductId,
            string productId,
            string? variantId,
            string productName,
            Guid shopId,
            string shopName,
            decimal livestreamPrice,
            decimal originalPrice,
            int stock,
            int quantity,
            string primaryImage,
            Dictionary<string, string>? attributes = null,
            string createdBy = "system")
        {
            LivestreamCartId = livestreamCartId;
            LivestreamId = livestreamId;
            LivestreamProductId = livestreamProductId;
            ProductId = productId;
            VariantId = variantId;
            ProductName = productName;
            ShopId = shopId;
            ShopName = shopName;
            LivestreamPrice = livestreamPrice;
            OriginalPrice = originalPrice;
            Stock = stock;
            Quantity = quantity;
            PrimaryImage = primaryImage;
            Attributes = attributes ?? new Dictionary<string, string>();
            ProductStatus = true;
            SetCreator(createdBy);
        }

        public void UpdateQuantity(int newQuantity, string modifiedBy)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0");

            Quantity = newQuantity;
            SetModifier(modifiedBy);
        }

        public void UpdatePrice(decimal newPrice, string modifiedBy)
        {
            if (newPrice < 0)
                throw new ArgumentException("Price cannot be negative");

            LivestreamPrice = newPrice;
            SetModifier(modifiedBy);
        }

        public decimal TotalPrice => LivestreamPrice * Quantity;
        public decimal DiscountPercentage => OriginalPrice > 0 ?
            Math.Round((OriginalPrice - LivestreamPrice) / OriginalPrice * 100, 2) : 0;

        public override bool IsValid()
        {
            return LivestreamCartId != Guid.Empty &&
                   LivestreamId != Guid.Empty &&
                   LivestreamProductId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(ProductId) &&
                   !string.IsNullOrWhiteSpace(ProductName) &&
                   ShopId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(ShopName) &&
                   LivestreamPrice >= 0 &&
                   OriginalPrice >= 0 &&
                   Stock >= 0 &&
                   Quantity > 0;
        }
    }
}