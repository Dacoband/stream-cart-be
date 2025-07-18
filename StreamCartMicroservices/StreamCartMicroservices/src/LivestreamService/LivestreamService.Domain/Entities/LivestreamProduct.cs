using Shared.Common.Domain.Bases;
using System;

namespace LivestreamService.Domain.Entities
{
    public class LivestreamProduct : BaseEntity
    {
        public Guid LivestreamId { get; private set; }
        public string ProductId { get; private set; }
        public string VariantId { get; private set; }
        public Guid? FlashSaleId { get; private set; }
        public bool IsPin { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public int DisplayOrder { get; private set; }

        // Private constructor for EF Core
        private LivestreamProduct() { }

        // ✅ UPDATED CONSTRUCTOR - Thêm displayOrder parameter
        public LivestreamProduct(
            Guid livestreamId,
            string productId,
            string variantId,
            decimal price,
            int stock,
            Guid? flashSaleId = null,
            bool isPin = false,
            int displayOrder = 0,
            string createdBy = "system")
        {
            LivestreamId = livestreamId;
            ProductId = productId;
            VariantId = variantId;
            Price = price;
            Stock = stock;
            FlashSaleId = flashSaleId;
            IsPin = isPin;
            DisplayOrder = displayOrder;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }

        public void UpdatePrice(decimal price, string modifiedBy)
        {
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

        public void SetFlashSale(Guid? flashSaleId, string modifiedBy)
        {
            FlashSaleId = flashSaleId;
            SetModifier(modifiedBy);
        }

        public void UpdateDisplayOrder(int displayOrder, string modifiedBy)
        {
            DisplayOrder = displayOrder;
            SetModifier(modifiedBy);
        }

        public override bool IsValid()
        {
            return LivestreamId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(ProductId) &&
                   Price >= 0 &&
                   Stock >= 0 &&
                   DisplayOrder >= 0;
        }
    }
}