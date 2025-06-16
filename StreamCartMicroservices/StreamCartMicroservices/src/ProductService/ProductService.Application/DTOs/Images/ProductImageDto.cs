using System;

namespace ProductService.Application.DTOs.Images
{
    public class ProductImageDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string AltText { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }

    public class CreateProductImageDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string AltText { get; set; } = string.Empty;
    }

    public class UpdateProductImageDto
    {
        public bool? IsPrimary { get; set; }
        public int? DisplayOrder { get; set; }
        public string AltText { get; set; }
    }

    public class SetPrimaryImageDto
    {
        public bool IsPrimary { get; set; }
    }

    public class ReorderImagesDto
    {
        public List<ImageOrderItem> ImagesOrder { get; set; } = new List<ImageOrderItem>();
    }

    public class ImageOrderItem
    {
        public Guid ImageId { get; set; }
        public int DisplayOrder { get; set; }
    }
}