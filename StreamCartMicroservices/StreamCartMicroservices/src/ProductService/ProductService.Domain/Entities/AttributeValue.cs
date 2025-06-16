using System;
using Shared.Common.Domain.Bases;

namespace ProductService.Domain.Entities
{
    public class AttributeValue : BaseEntity
    {
        public Guid AttributeId { get; private set; }
        public string ValueName { get; private set; }

        // Required by EF Core
        private AttributeValue() { }

        public AttributeValue(Guid attributeId, string valueName, string createdBy = "system")
        {
            if (attributeId == Guid.Empty)
                throw new ArgumentException("AttributeId cannot be empty", nameof(attributeId));

            if (string.IsNullOrWhiteSpace(valueName))
                throw new ArgumentException("Value name cannot be empty", nameof(valueName));

            AttributeId = attributeId;
            ValueName = valueName;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }

        public void UpdateValueName(string valueName)
        {
            if (string.IsNullOrWhiteSpace(valueName))
                throw new ArgumentException("Value name cannot be empty", nameof(valueName));

            ValueName = valueName;
        }

        public void SetUpdatedBy(string updatedBy)
        {
            SetModifier(updatedBy);
        }

        public override bool IsValid()
        {
            return AttributeId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(ValueName);
        }
    }
}