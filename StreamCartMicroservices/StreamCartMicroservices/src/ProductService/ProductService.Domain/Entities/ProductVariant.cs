using System;
using Shared.Common.Domain.Bases;

namespace ProductService.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string SKU { get; private set; }
        public decimal Price { get; private set; }
        public decimal? FlashSalePrice { get; private set; }
        public int Stock { get; private set; }
        public virtual ICollection<FlashSale> FlashSales { get; set; } = new List<FlashSale>();


        // Required by EF Core
        private ProductVariant() { }

        public ProductVariant(
            Guid productId,
            string sku,
            decimal price,
            int stock,
            string createdBy = "system")
        {
            ProductId = productId;
            SKU = sku;
            Price = price;
            Stock = stock;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }

        public void UpdateSKU(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU cannot be empty", nameof(sku));

            SKU = sku;
        }

        public void UpdatePrice(decimal price, decimal? flashSalePrice = null)
        {
            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero", nameof(price));

            if (flashSalePrice.HasValue && flashSalePrice.Value < 0)
                throw new ArgumentException("Flash sale price must be greater than zero", nameof(flashSalePrice));

            if (flashSalePrice.HasValue && flashSalePrice.Value >= price)
                throw new ArgumentException("Flash sale price must be less than the regular price", nameof(flashSalePrice));

            Price = price;
            FlashSalePrice = flashSalePrice;
        }

        public void UpdateStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));

            Stock = quantity;
        }

        public void DecrementStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            if (quantity > Stock)
                throw new InvalidOperationException("Insufficient stock");

            Stock -= quantity;
        }

        public void IncrementStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            Stock += quantity;
        }

        public bool HasSufficientStock(int requestedQuantity)
        {
            return Stock >= requestedQuantity;
        }

        public decimal GetCurrentPrice()
        {
            return FlashSalePrice ?? Price;
        }

        public void SetUpdatedBy(string updatedBy)
        {
            SetModifier(updatedBy);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(SKU) &&
                   Price > 0 &&
                   Stock >= 0 &&
                   ProductId != Guid.Empty;
        }
    }
}