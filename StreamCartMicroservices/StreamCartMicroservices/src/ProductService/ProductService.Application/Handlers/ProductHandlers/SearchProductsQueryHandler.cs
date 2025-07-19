using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.ProductQueries;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Domain.Bases;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, SearchProductResponseDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IShopServiceClient _shopServiceClient; // ✅ Add ShopServiceClient

        public SearchProductsQueryHandler(
            IProductRepository productRepository,
            IProductImageRepository productImageRepository,
            ICategoryRepository categoryRepository,
            IShopServiceClient shopServiceClient) // ✅ Inject ShopServiceClient
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _categoryRepository = categoryRepository;
            _shopServiceClient = shopServiceClient; // ✅ Initialize ShopServiceClient
        }

        public async Task<SearchProductResponseDto> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Get all products first
                var allProducts = await _productRepository.GetAllAsync();

                // Apply filters
                var filteredProducts = allProducts.Where(p => !p.IsDeleted && p.IsActive);

                // Search by term
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower().Trim();
                    filteredProducts = filteredProducts.Where(p =>
                        p.ProductName.ToLower().Contains(searchTerm) ||
                        p.Description.ToLower().Contains(searchTerm) ||
                        p.SKU.ToLower().Contains(searchTerm));
                }

                // Apply filters
                if (request.CategoryId.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.CategoryId == request.CategoryId.Value);
                }

                if (request.ShopId.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => p.ShopId == request.ShopId.Value);
                }

                if (request.MinPrice.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => GetFinalPrice(p) >= request.MinPrice.Value);
                }

                if (request.MaxPrice.HasValue)
                {
                    filteredProducts = filteredProducts.Where(p => GetFinalPrice(p) <= request.MaxPrice.Value);
                }

                if (request.InStockOnly)
                {
                    filteredProducts = filteredProducts.Where(p => p.StockQuantity > 0);
                }

                if (request.OnSaleOnly)
                {
                    filteredProducts = filteredProducts.Where(p => p.DiscountPrice.HasValue && p.DiscountPrice.Value > 0);
                }

                // Apply sorting
                filteredProducts = ApplySorting(filteredProducts, request.SortBy);

                var totalResults = filteredProducts.Count();

                // Apply pagination
                var pagedProducts = filteredProducts
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Convert to DTOs with REAL shop data
                var productDtos = new List<ProductSearchItemDto>();

                // ✅ Group products by ShopId to reduce API calls
                var productsByShop = pagedProducts.GroupBy(p => p.ShopId);
                var shopDataCache = new Dictionary<Guid, ShopDto?>();

                foreach (var shopGroup in productsByShop)
                {
                    // ✅ Get shop data once per shop (not per product)
                    ShopDto? shopData = null;
                    if (shopGroup.Key.HasValue && shopGroup.Key != Guid.Empty)
                    {
                        if (!shopDataCache.ContainsKey(shopGroup.Key.Value))
                        {
                            try
                            {
                                shopData = await _shopServiceClient.GetShopByIdAsync(shopGroup.Key.Value);
                                shopDataCache[shopGroup.Key.Value] = shopData;
                            }
                            catch (Exception ex)
                            {
                                // Log error but continue with fallback data
                                shopData = null;
                                shopDataCache[shopGroup.Key.Value] = null;
                            }
                        }
                        else
                        {
                            shopData = shopDataCache[shopGroup.Key.Value];
                        }
                    }

                    // Process products for this shop
                    foreach (var product in shopGroup)
                    {
                        var primaryImage = await _productImageRepository.GetPrimaryImageAsync(product.Id);

                        // ✅ Handle nullable CategoryId safely
                        var categoryId = product.CategoryId ?? Guid.Empty;
                        Category? category = null;
                        if (product.CategoryId.HasValue)
                        {
                            category = await _categoryRepository.GetByIdAsync(product.CategoryId.Value.ToString());
                        }

                        var finalPrice = GetFinalPrice(product);
                        var discountPercentage = product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0
                            ? product.DiscountPrice.Value
                            : 0;

                        productDtos.Add(new ProductSearchItemDto
                        {
                            Id = product.Id,
                            ProductName = product.ProductName,
                            Description = product.Description,
                            BasePrice = product.BasePrice,
                            DiscountPrice = product.DiscountPrice,
                            FinalPrice = finalPrice,
                            StockQuantity = product.StockQuantity,
                            PrimaryImageUrl = primaryImage?.ImageUrl,
                            QuantitySold = product.QuantitySold,
                            DiscountPercentage = discountPercentage,
                            IsOnSale = product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0,
                            InStock = product.StockQuantity > 0,
                            ShopId = product.ShopId ?? Guid.Empty, // ✅ Handle nullable ShopId with fallback
                            ShopName = shopData?.ShopName ?? "Unknown Shop", // ✅ Real shop name from API
                            ShopLocation = "Location Not Available", // ✅ Could be enhanced if shop has address field
                            //ShopRating = shopData?.RatingAverage ?? 0m, // ✅ Real shop rating from API
                            CategoryId = categoryId, // ✅ Use the safe CategoryId value
                            CategoryName = category?.CategoryName ?? "Unknown",
                            AverageRating = 4.0m, // TODO: Calculate from reviews when review service is available
                            //ReviewCount = shopData?.TotalReview ?? 0, // ✅ Real review count from shop
                            HighlightedName = HighlightSearchTerm(product.ProductName, request.SearchTerm)
                        });
                    }
                }

                stopwatch.Stop();

                return new SearchProductResponseDto
                {
                    Products = new PagedResult<ProductSearchItemDto>
                    {
                        Items = productDtos,
                        CurrentPage = request.PageNumber,
                        PageSize = request.PageSize,
                        TotalCount = totalResults
                    },
                    TotalResults = totalResults,
                    SearchTerm = request.SearchTerm,
                    SearchTimeMs = stopwatch.Elapsed.TotalMilliseconds,
                    SuggestedKeywords = GenerateSuggestedKeywords(request.SearchTerm),
                    AppliedFilters = new SearchFiltersDto
                    {
                        CategoryId = request.CategoryId,
                        MinPrice = request.MinPrice,
                        MaxPrice = request.MaxPrice,
                        ShopId = request.ShopId,
                        SortBy = request.SortBy,
                        InStockOnly = request.InStockOnly,
                        MinRating = request.MinRating,
                        OnSaleOnly = request.OnSaleOnly
                    }
                };
            }
            catch (Exception)
            {
                stopwatch.Stop();
                throw;
            }
        }

        private decimal GetFinalPrice(Domain.Entities.Product product)
        {
            if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
            {
                return product.BasePrice * (1 - (product.DiscountPrice.Value / 100));
            }
            return product.BasePrice;
        }

        private IEnumerable<Domain.Entities.Product> ApplySorting(IEnumerable<Domain.Entities.Product> products, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "price_asc" => products.OrderBy(p => GetFinalPrice(p)),
                "price_desc" => products.OrderByDescending(p => GetFinalPrice(p)),
                "newest" => products.OrderByDescending(p => p.CreatedAt),
                "best_selling" => products.OrderByDescending(p => p.QuantitySold),
                "name_asc" => products.OrderBy(p => p.ProductName),
                "name_desc" => products.OrderByDescending(p => p.ProductName),
                _ => products.OrderByDescending(p => p.CreatedAt) // Default to newest
            };
        }

        private string HighlightSearchTerm(string text, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return text;

            var pattern = Regex.Escape(searchTerm);
            return Regex.Replace(text, pattern, $"<mark>$0</mark>", RegexOptions.IgnoreCase);
        }

        private List<string> GenerateSuggestedKeywords(string searchTerm)
        {
            // Simple suggestion logic - in reality, this would use more sophisticated algorithms
            var suggestions = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                suggestions.AddRange(new[]
                {
                    $"{searchTerm} giá rẻ",
                    $"{searchTerm} chất lượng",
                    $"{searchTerm} sale",
                    $"{searchTerm} hot"
                });
            }

            return suggestions.Take(4).ToList();
        }
    }
}