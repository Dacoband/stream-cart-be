using System;
using System.Collections.Generic;

namespace ProductService.Application.DTOs.Combinations
{
    public class ProductCombinationDto
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }
        public string? AttributeName { get; set; }
        public string? ValueName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}