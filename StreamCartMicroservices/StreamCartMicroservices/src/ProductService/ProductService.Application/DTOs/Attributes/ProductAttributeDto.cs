using System;

namespace ProductService.Application.DTOs.Attributes
{
    public class ProductAttributeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public class CreateProductAttributeDto
    {
        public string Name { get; set; }
    }

    public class UpdateProductAttributeDto
    {
        public string Name { get; set; }
    }

    public class AttributeValueDto
    {
        public Guid Id { get; set; }
        public Guid AttributeId { get; set; }
        public string ValueName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public class CreateAttributeValueDto
    {
        public Guid AttributeId { get; set; }
        public string ValueName { get; set; }
    }

    public class UpdateAttributeValueDto
    {
        public string ValueName { get; set; }
    }
}