using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;

namespace ProductService.Application.Queries.ProductQueries
{
    public class SearchProductsQuery : IRequest<SearchProductResponseDto>
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public Guid? ShopId { get; set; }
        public string SortBy { get; set; } = "relevance";
        public bool InStockOnly { get; set; } = false;
        public int? MinRating { get; set; }
        public bool OnSaleOnly { get; set; } = false;
    }
}