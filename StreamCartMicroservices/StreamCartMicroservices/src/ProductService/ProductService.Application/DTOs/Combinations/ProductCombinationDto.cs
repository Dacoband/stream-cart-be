using System;
using System.Collections.Generic;

namespace ProductService.Application.DTOs.Combinations
{
    public class ProductCombinationDto
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }
        public string AttributeName { get; set; }
        public string ValueName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public class CreateProductCombinationDto
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }
    }

    public class UpdateProductCombinationDto
    {
        public Guid AttributeValueId { get; set; }
    }

    public class GenerateCombinationsDto
    {
        public List<AttributeValueGroup> AttributeValueGroups { get; set; } = new List<AttributeValueGroup>();
        public decimal DefaultPrice { get; set; }
        public int DefaultStock { get; set; }
    }

    public class AttributeValueGroup
    {
        public Guid AttributeId { get; set; }
        public List<Guid> AttributeValueIds { get; set; } = new List<Guid>();
    }
}