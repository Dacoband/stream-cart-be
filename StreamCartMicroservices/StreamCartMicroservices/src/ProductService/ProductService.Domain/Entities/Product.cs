using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Domain.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string ProductName { get; private set; }

        [StringLength(2000)]
        public string Description { get; private set; }

        [StringLength(50)]
        public string SKU { get; private set; }

        public Guid? CategoryId { get; private set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; private set; }
            
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; private set; }

        public int StockQuantity { get; private set; }

        public bool IsActive { get; private set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Weight { get; private set; }

        [StringLength(100)]
        public string Dimensions { get; private set; }

        public bool HasVariant { get; private set; }

        public int QuantitySold { get; private set; }

        public Guid? ShopId { get; private set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public virtual ICollection<FlashSale> FlashSales { get; set; } = new List<FlashSale>();


        // EF Constructor
        private Product() { }

        // Public constructor
        public Product(
            string productName,
            string description,
            string sku,
            Guid? categoryId,
            decimal basePrice,
            int stockQuantity,
            Guid? shopId)
        {
            ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
            Description = description;
            SKU = sku;
            CategoryId = categoryId;
            BasePrice = basePrice;
            StockQuantity = stockQuantity;
            ShopId = shopId;
            IsActive = true;
            QuantitySold = 0;
            HasVariant = false;
        }

        // Domain methods
        public void UpdateBasicInfo(string productName, string description, string sku, Guid? categoryId)
        {
            if (!string.IsNullOrWhiteSpace(productName))
                ProductName = productName;

            Description = description;

            if (!string.IsNullOrWhiteSpace(sku))
                SKU = sku;

            CategoryId = categoryId;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdatePricing(decimal basePrice, decimal? discountPrice)
        {
            if (basePrice < 0)
                throw new ArgumentException("Base price cannot be negative", nameof(basePrice));

            if (discountPrice.HasValue && discountPrice.Value < 0)
                throw new ArgumentException("Discount price cannot be negative", nameof(discountPrice));

            if (discountPrice.HasValue && discountPrice.Value > basePrice)
                throw new ArgumentException("Discount price cannot be greater than base price", nameof(discountPrice));

            BasePrice = basePrice;
            DiscountPrice = discountPrice;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdateStock(int stockQuantity)
        {
            if (stockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));

            StockQuantity = stockQuantity;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void AddStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity to add cannot be negative", nameof(quantity));

            StockQuantity += quantity;
            LastModifiedAt = DateTime.UtcNow;
        }

        public bool RemoveStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity to remove cannot be negative", nameof(quantity));

            if (StockQuantity < quantity)
                return false;

            StockQuantity -= quantity;
            QuantitySold += quantity;
            LastModifiedAt = DateTime.UtcNow;
            return true;
        }

        public void Activate()
        {
            IsActive = true;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void UpdatePhysicalAttributes(decimal? weight, string dimensions)
        {
            Weight = weight;
            Dimensions = dimensions;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void SetHasVariant(bool hasVariant)
        {
            HasVariant = hasVariant;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void AssignToShop(Guid shopId)
        {
            ShopId = shopId;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void SetUpdatedBy(string updatedBy)
        {
            LastModifiedBy = updatedBy;
            LastModifiedAt = DateTime.UtcNow;
        }

        public bool HasSufficientStock(int requestedQuantity)
        {
            return StockQuantity >= requestedQuantity;
        }
    }
}