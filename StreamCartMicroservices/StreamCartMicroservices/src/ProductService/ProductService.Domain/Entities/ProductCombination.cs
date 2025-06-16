using System;
using Shared.Common.Domain.Bases;

namespace ProductService.Domain.Entities
{
    public class ProductCombination : BaseEntity
    {
        public Guid VariantId { get; private set; }
        public Guid AttributeValueId { get; private set; }

        // Required by EF Core
        private ProductCombination() { }

        public ProductCombination(Guid variantId, Guid attributeValueId, string createdBy = "system")
        {
            if (variantId == Guid.Empty)
                throw new ArgumentException("VariantId cannot be empty", nameof(variantId));

            if (attributeValueId == Guid.Empty)
                throw new ArgumentException("AttributeValueId cannot be empty", nameof(attributeValueId));

            VariantId = variantId;
            AttributeValueId = attributeValueId;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }

        public void UpdateAttributeValue(Guid attributeValueId)
        {
            if (attributeValueId == Guid.Empty)
                throw new ArgumentException("AttributeValueId cannot be empty", nameof(attributeValueId));

            AttributeValueId = attributeValueId;
        }

        public void SetUpdatedBy(string updatedBy)
        {
            SetModifier(updatedBy);
        }

        public override bool IsValid()
        {
            return VariantId != Guid.Empty &&
                   AttributeValueId != Guid.Empty;
        }
    }
}