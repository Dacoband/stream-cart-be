using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs.Products
{
    public class CompleteProductDto
    {
        // Base product information
        [Required]
        public string? ProductName { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public string? SKU { get; set; }
        public Guid? CategoryId { get; set; }
        [Required]
        public decimal BasePrice { get; set; }
        [Required]
        public int StockQuantity { get; set; }

        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public bool HasVariant { get; set; }
        public Guid? ShopId { get; set; }

        // Product images
        public List<ProductImageDto> Images { get; set; } = new();

        // Product attributes and variants
        public List<ProductAttributeDto> Attributes { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
    }

    public class ProductImageDto
    {
        [Required]
        public string? ImageUrl { get; set; }
        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string AltText { get; set; } = string.Empty;
    }

    public class ProductAttributeDto
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public List<string> Values { get; set; } = new();
    }

    public class ProductVariantDto
    {
        [Required]
        public string? SKU { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Stock { get; set; }
        [Required]
        public List<VariantAttributeDto> Attributes { get; set; } = new();
    }

    public class VariantAttributeDto
    {
        [Required]
        public string? AttributeName { get; set; }
        [Required]
        public string? AttributeValue { get; set; }
    }
}