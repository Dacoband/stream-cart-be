using Shared.Common.Domain.Bases;
using System;

namespace LivestreamService.Domain.Entities
{
    public class LivestreamProduct : BaseEntity
    {
        public Guid LivestreamId { get; private set; }
        public string ProductId { get; private set; }
        public string VariantId { get; private set; }
       // public Guid? FlashSaleId { get; private set; }
        public bool IsPin { get; private set; }
        public decimal Price { get; private set; }
        public decimal OriginalPrice { get; private set; } // Giá gốc từ sản phẩm
        public int Stock { get; private set; }
        //public int DisplayOrder { get; private set; }

        // Private constructor for EF Core
        private LivestreamProduct() { }

        // ✅ UPDATED CONSTRUCTOR - Thêm displayOrder parameter
        public LivestreamProduct(
            Guid livestreamId,
            string productId,
            string variantId,
            decimal price,
            decimal originalPrice,
            int stock,
            bool isPin = false,
            string createdBy = "system")
        {
            LivestreamId = livestreamId;
            ProductId = productId;
            VariantId = variantId;
            Price = price;
            OriginalPrice = originalPrice;
            Stock = stock;
            IsPin = isPin;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }
        public void UpdateOriginalPrice(decimal originalPrice, string modifiedBy)
        {
            if (originalPrice < 0)
            {
                throw new ArgumentException("Original price cannot be negative");
            }

            OriginalPrice = originalPrice;
            SetModifier(modifiedBy);
        }
        public void UpdatePrice(decimal price, string modifiedBy)
        {
            if (price < 0)
            {
                throw new ArgumentException("Price cannot be negative");
            }

            Price = price;
            SetModifier(modifiedBy);
        }

        public void UpdateStock(int stock, string modifiedBy)
        {
            Stock = stock;
            SetModifier(modifiedBy);
        }

        public void SetPin(bool isPin, string modifiedBy)
        {
            IsPin = isPin;
            SetModifier(modifiedBy);
        }
        public decimal DiscountPercentage
        {
            get
            {
                if (OriginalPrice == 0) return 0;
                if (Price >= OriginalPrice) return 0;

                return Math.Round((OriginalPrice - Price) / OriginalPrice * 100, 2);
            }
        }
        public bool HasDiscount => Price < OriginalPrice && OriginalPrice > 0;


        public override bool IsValid()
        {
            return LivestreamId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(ProductId) &&
                   Price >= 0 &&
                   Stock >= 0 
                   ;
        }
    }
}