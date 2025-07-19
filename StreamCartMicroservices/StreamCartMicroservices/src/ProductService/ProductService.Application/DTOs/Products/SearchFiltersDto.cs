namespace ProductService.Application.DTOs.Products
{
    public class SearchFiltersDto
    {
        public Guid? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public Guid? ShopId { get; set; }
        public string SortBy { get; set; } = string.Empty;
        public bool InStockOnly { get; set; }
        public int? MinRating { get; set; }
        public bool OnSaleOnly { get; set; }
    }
}