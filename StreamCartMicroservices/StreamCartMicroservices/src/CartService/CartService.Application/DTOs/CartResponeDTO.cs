using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class CartResponeDTO
    {
        public Guid CartId { get; set; }
        public string CustomerId { get; set; }
        public int TotalProduct { get; set; }
        public List<ProductInShopCart> CartItemByShop {  get; set; }
    }
    public class PriceData
    {
       public decimal CurrentPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal Discount { get; set; }
    }
    public class ProductCart
    {
        public Guid CartItemId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantID { get; set; }
        public string ProductName { get; set; }
        public PriceData PriceData { get; set; }
        public int Quantity { get; set; }
        public string PrimaryImage { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public int StockQuantity { get; set; }
        public bool ProductStatus { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
    }
    public class ProductInShopCart
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; }
        public List<ProductCart> Products { get; set; }
        public int NumberOfProduct {  get; set; }
        public decimal TotalPriceInShop { get; set; }
    }
}
