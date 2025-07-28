using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class ProductDetailDto
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int StockQuantity { get; set; }
        public int QuantitySold { get; set; }
        public string? Weight { get; set; }
        //public string? Dimension { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public List<string> PrimaryImage { get; set; } = new();

        // Shop information
        public Guid ShopId { get; set; }
        public string? ShopName { get; set; }
        public DateTime ShopStartTime { get; set; }
        public decimal ShopCompleteRate { get; set; }
        public int ShopTotalReview { get; set; }
        public decimal ShopRatingAverage { get; set; }
        public string? ShopLogo { get; set; }
        public int ShopTotalProduct { get; set; }

        // Attributes and variants
        public List<ProductDetailAttributeDto> Attributes { get; set; } = new();
        public List<ProductDetailVariantDto> Variants { get; set; } = new();
    }

    //public class ProductDetailAttributeDto
    //{
    //    public string? AttributeName { get; set; }
    //    public List<string> Values { get; set; } = new();
    //    public List<string> ImageUrls { get; set; } = new(); 

    //}
    public class ProductDetailAttributeDto
    {
        public string? AttributeName { get; set; }

        // Replace separate lists with a list of pairs
        public List<AttributeValueImagePair> ValueImagePairs { get; set; } = new();
    }

    public class AttributeValueImagePair
    {
        public string Value { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
    public class ProductDetailVariantDto
    {
        public Guid VariantId { get; set; }
        public Dictionary<string, string> AttributeValues { get; set; } = new();
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public ProductDetailVariantImageDto? VariantImage { get; set; }
    }

    public class ProductDetailVariantImageDto
    {
        public Guid ImageId { get; set; }
        public string? Url { get; set; }
        public string? AltText { get; set; }
    }
}
