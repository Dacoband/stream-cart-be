using System;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int ReserveStock { get; private set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Weight { get;  set; }

        //[StringLength(100)]
        //public string Dimensions { get; private set; }
        // With these three properties:
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Length { get;  set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Width { get;  set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Height { get;  set; }


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
            ReserveStock = 0;
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
        public int GetTotalStock()
        {
            return Stock + ReserveStock;
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
                   ReserveStock >= 0 &&
                   ProductId != Guid.Empty;
        }
        public void ReserveStockForFlashSale(int quantity, string modifiedBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Reserve quantity must be greater than zero", nameof(quantity));

            if (quantity > Stock)
                throw new InvalidOperationException($"Cannot reserve {quantity} items. Only {Stock} available in stock");

            Stock -= quantity;
            ReserveStock += quantity;
            SetModifier(modifiedBy);
        }
        public bool CanReserveStock(int requestedQuantity)
        {
            return Stock >= requestedQuantity && requestedQuantity > 0;
        }
        public void ReleaseReservedStock(int quantity, string modifiedBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Release quantity must be greater than zero", nameof(quantity));

            //if (quantity > ReserveStock)
            //    throw new InvalidOperationException($"Cannot release {quantity} items. Only {ReserveStock} reserved");

            ReserveStock -= quantity;
            Stock += quantity;
            SetModifier(modifiedBy);
        }
        public void UseReservedStock(int quantity, string modifiedBy)
        {
            if (quantity <= 0)
                throw new ArgumentException("Use quantity must be greater than zero", nameof(quantity));

            if (quantity > ReserveStock)
                throw new InvalidOperationException($"Cannot use {quantity} items. Only {ReserveStock} reserved");

            ReserveStock -= quantity;
            SetModifier(modifiedBy);
        }
    }
}