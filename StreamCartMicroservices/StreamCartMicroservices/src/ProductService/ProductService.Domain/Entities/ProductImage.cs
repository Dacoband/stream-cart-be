using System;
using Shared.Common.Domain.Bases;

namespace ProductService.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Guid? VariantId { get; private set; }
        public string ImageUrl { get; private set; }
        public bool IsPrimary { get; private set; }
        public int DisplayOrder { get; private set; }
        public string AltText { get; private set; }

        // Required for EF Core
        private ProductImage() { }

        public ProductImage(
            Guid productId,
            string imageUrl,
            Guid? variantId = null,
            bool isPrimary = false,
            int displayOrder = 0,
            string altText = "",
            string createdBy = "system")
        {
            ProductId = productId;
            ImageUrl = imageUrl;
            VariantId = variantId;
            IsPrimary = isPrimary;
            DisplayOrder = displayOrder;
            AltText = altText ?? string.Empty;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }

        public void SetPrimary(bool isPrimary)
        {
            IsPrimary = isPrimary;
        }

        public void UpdateDisplayOrder(int displayOrder)
        {
            DisplayOrder = displayOrder;
        }

        public void UpdateAltText(string altText)
        {
            AltText = altText ?? string.Empty;
        }

        public void SetUpdatedBy(string updatedBy)
        {
            SetModifier(updatedBy);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(ImageUrl) &&
                   ProductId != Guid.Empty;
        }
    }
}