using System;
using Shared.Common.Domain.Bases;

namespace ProductService.Domain.Entities
{
    public class ProductAttribute : BaseEntity
    {
        public string Name { get; private set; }

        // Required by EF Core
        private ProductAttribute() { }

        public ProductAttribute(string name, string createdBy = "system")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Attribute name cannot be empty", nameof(name));

            Name = name;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Attribute name cannot be empty", nameof(name));

            Name = name;
        }

        public void SetUpdatedBy(string updatedBy)
        {
            SetModifier(updatedBy);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }
    }
}