using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs.OrderDTOs;
using OrderService.Application.Interfaces.IRepositories;
using OrderService.Application.Interfaces.IServices;
using OrderService.Application.Queries.OrderQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Application.Handlers.OrderQueryHandlers
{
    /// <summary>
    /// Handler để lấy doanh thu và sản phẩm của livestream
    /// </summary>
    public class GetLivestreamRevenueQueryHandler : IRequestHandler<GetLivestreamRevenueQuery, LivestreamRevenueDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductServiceClient _productServiceClient;
        private readonly ILogger<GetLivestreamRevenueQueryHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public GetLivestreamRevenueQueryHandler(
            IOrderRepository orderRepository,
            IProductServiceClient productServiceClient,
            ILogger<GetLivestreamRevenueQueryHandler> logger,
            IHttpClientFactory httpClientFactory)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _productServiceClient = productServiceClient ?? throw new ArgumentNullException(nameof(productServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<LivestreamRevenueDto> Handle(GetLivestreamRevenueQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("📊 Getting revenue for livestream {LivestreamId}", request.LivestreamId);

                // 1. Lấy tất cả đơn hàng từ livestream
                var livestreamOrders = await _orderRepository.GetByLivestreamAsync(request.LivestreamId);
                var ordersList = livestreamOrders.ToList();

                _logger.LogInformation("🔍 Found {OrderCount} orders for livestream {LivestreamId}", ordersList.Count, request.LivestreamId);

                if (!ordersList.Any())
                {
                    _logger.LogInformation("No orders found for livestream {LivestreamId}", request.LivestreamId);
                    return new LivestreamRevenueDto
                    {
                        LivestreamId = request.LivestreamId,
                        TotalRevenue = 0,
                        TotalOrders = 0,
                        ProductsWithOrders = new List<LivestreamProductSalesDto>()
                    };
                }

                // 2. Tính tổng doanh thu và số đơn hàng
                var totalRevenue = ordersList.Sum(o => o.FinalAmount);
                var totalOrders = ordersList.Count;

                // 3. ✅ Lấy danh sách sản phẩm có đơn hàng từ API
                var productsWithOrders = await GetProductsWithOrdersFromApiAsync(ordersList);

                var result = new LivestreamRevenueDto
                {
                    LivestreamId = request.LivestreamId,
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    ProductsWithOrders = productsWithOrders
                };

                _logger.LogInformation("✅ Livestream {LivestreamId}: {Orders} orders, {Revenue:N0}đ, {Products} products/variants",
                    request.LivestreamId, totalOrders, totalRevenue, productsWithOrders.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting livestream revenue for {LivestreamId}", request.LivestreamId);
                throw;
            }
        }

        /// <summary>
        /// ✅ Lấy danh sách sản phẩm có đơn hàng từ API ORDER ITEMS
        /// </summary>
        private async Task<List<LivestreamProductSalesDto>> GetProductsWithOrdersFromApiAsync(List<Domain.Entities.Orders> orders)
        {
            try
            {
                _logger.LogInformation("🔍 Processing {OrderCount} orders to extract product sales data from API", orders.Count);

                var allOrderItems = new List<OrderItemApiResponse>();

                // ✅ Lấy order items từ API cho từng order
                foreach (var order in orders)
                {
                    try
                    {
                        _logger.LogInformation("📋 Getting order items for Order {OrderId}", order.Id);

                        var orderItems = await GetOrderItemsByOrderIdAsync(order.Id);
                        if (orderItems != null && orderItems.Any())
                        {
                            allOrderItems.AddRange(orderItems);
                            _logger.LogInformation("✅ Got {ItemCount} items for order {OrderId}", orderItems.Count, order.Id);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ No items found for order {OrderId}", order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to get items for order {OrderId}", order.Id);
                    }
                }

                _logger.LogInformation("📦 Total order items across all orders: {ItemCount}", allOrderItems.Count);

                if (!allOrderItems.Any())
                {
                    _logger.LogWarning("⚠️ No order items found for any orders!");
                    return new List<LivestreamProductSalesDto>();
                }

                // ✅ Group theo ProductId và VariantId
                var productVariantSales = allOrderItems
                    .GroupBy(item => new { item.ProductId, item.VariantId })
                    .Select(g => new
                    {
                        ProductId = g.Key.ProductId,
                        VariantId = g.Key.VariantId,
                        QuantitySold = g.Sum(item => item.Quantity),
                        Revenue = g.Sum(item => item.TotalPrice),
                        UnitPrice = g.FirstOrDefault()?.UnitPrice ?? 0
                    })
                    .OrderByDescending(p => p.Revenue)
                    .ToList();

                _logger.LogInformation("📊 Found {ProductVariantCount} unique product/variant combinations", productVariantSales.Count);

                var productsWithOrders = new List<LivestreamProductSalesDto>();

                // ✅ Lấy thông tin chi tiết sản phẩm
                foreach (var productSale in productVariantSales)
                {
                    try
                    {
                        _logger.LogInformation("🔍 Getting details for ProductId: {ProductId}, VariantId: {VariantId}",
                            productSale.ProductId, productSale.VariantId?.ToString() ?? "NULL");

                        string productName = "Unknown Product";
                        string variantName = null;
                        string variantSKU = null;
                        string productImageUrl = string.Empty;

                        // ✅ CASE 1: Có VariantId - Call Variant API
                        if (productSale.VariantId.HasValue)
                        {
                            var variantInfo = await GetVariantDetailsAsync(productSale.VariantId.Value);
                            if (variantInfo != null)
                            {
                                productName = variantInfo.ProductName ?? "Unknown Product";
                                variantName = variantInfo.VariantName ?? $"Variant {productSale.VariantId.Value.ToString()[..8]}";
                                variantSKU = variantInfo.SKU;
                                productImageUrl = variantInfo.ImageUrl ?? string.Empty;

                                _logger.LogInformation("✅ Got variant info: {ProductName} - {VariantName}", productName, variantName);
                            }
                            else
                            {
                                // Fallback: Call Product API
                                var productInfo = await GetProductDetailsAsync(productSale.ProductId);
                                if (productInfo != null)
                                {
                                    productName = productInfo.ProductName ?? "Unknown Product";
                                    productImageUrl = productInfo.PrimaryImageUrl ?? string.Empty;
                                }
                                variantName = $"Variant {productSale.VariantId.Value.ToString()[..8]}";
                                _logger.LogWarning("⚠️ Variant not found, using product info with fallback variant name");
                            }
                        }
                        // ✅ CASE 2: Không có VariantId - Call Product API
                        else
                        {
                            var productInfo = await GetProductDetailsAsync(productSale.ProductId);
                            if (productInfo != null)
                            {
                                productName = productInfo.ProductName ?? "Unknown Product";
                                productImageUrl = productInfo.PrimaryImageUrl ?? string.Empty;
                                variantSKU = productInfo.SKU;

                                _logger.LogInformation("✅ Got product info: {ProductName}", productName);
                            }
                            else
                            {
                                _logger.LogWarning("⚠️ Product not found: {ProductId}", productSale.ProductId);
                                continue; // Skip sản phẩm không tìm thấy
                            }
                        }

                        var salesDto = new LivestreamProductSalesDto
                        {
                            ProductId = productSale.ProductId,
                            VariantId = productSale.VariantId,
                            ProductName = productName,
                            VariantName = variantName,
                            VariantSKU = variantSKU,
                            ProductImageUrl = productImageUrl,
                            QuantitySold = productSale.QuantitySold,
                            Revenue = productSale.Revenue,
                            UnitPrice = productSale.UnitPrice
                        };

                        productsWithOrders.Add(salesDto);

                        _logger.LogInformation("✅ Added: {ProductName} - Qty: {Quantity}, Revenue: {Revenue:N0}đ",
                            salesDto.ProductName, salesDto.QuantitySold, salesDto.Revenue);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to get details for ProductId {ProductId}, VariantId {VariantId}. Adding with basic info.",
                            productSale.ProductId, productSale.VariantId);

                        // ✅ Thêm với thông tin cơ bản nếu không lấy được chi tiết
                        var fallbackDto = new LivestreamProductSalesDto
                        {
                            ProductId = productSale.ProductId,
                            VariantId = productSale.VariantId,
                            ProductName = "Unknown Product",
                            VariantName = productSale.VariantId.HasValue ? $"Variant {productSale.VariantId.Value.ToString()[..8]}" : null,
                            VariantSKU = null,
                            ProductImageUrl = string.Empty,
                            QuantitySold = productSale.QuantitySold,
                            Revenue = productSale.Revenue,
                            UnitPrice = productSale.UnitPrice
                        };

                        productsWithOrders.Add(fallbackDto);
                    }
                }

                _logger.LogInformation("✅ Successfully processed {Count} products with orders", productsWithOrders.Count);
                return productsWithOrders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing products with orders from API");
                return new List<LivestreamProductSalesDto>();
            }
        }

        /// <summary>
        /// ✅ GET ORDER ITEMS by Order ID từ API
        /// </summary>
        /// <summary>
        /// ✅ GET ORDER ITEMS by Order ID từ API
        /// </summary>
        private async Task<List<OrderItemApiResponse>?> GetOrderItemsByOrderIdAsync(Guid orderId)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://brightpa.me/api/order-items/by-order/{orderId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("📥 Order items API response for {OrderId}: {Json}", orderId, json);

                    // ✅ FIX: API trả về array trực tiếp, không có wrapper ApiResponse
                    var orderItems = JsonSerializer.Deserialize<List<OrderItemApiResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return orderItems ?? new List<OrderItemApiResponse>();
                }
                else
                {
                    _logger.LogWarning("Failed to get order items for {OrderId}: {StatusCode}", orderId, response.StatusCode);
                    return new List<OrderItemApiResponse>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling order items API for {OrderId}", orderId);
                return new List<OrderItemApiResponse>();
            }
        }

        /// <summary>
        /// ✅ GET PRODUCT DETAILS từ API trực tiếp
        /// </summary>
        private async Task<ProductApiResponse?> GetProductDetailsAsync(Guid productId)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://brightpa.me/api/products/{productId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProductApiResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to get product {ProductId}: {StatusCode}", productId, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling product API for {ProductId}", productId);
                return null;
            }
        }

        /// <summary>
        /// ✅ GET VARIANT DETAILS từ API trực tiếp
        /// </summary>
        private async Task<VariantApiResponse?> GetVariantDetailsAsync(Guid variantId)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://brightpa.me/api/product-variants/{variantId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<VariantApiResponse>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Data;
                }
                else
                {
                    _logger.LogWarning("Failed to get variant {VariantId}: {StatusCode}", variantId, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling variant API for {VariantId}", variantId);
                return null;
            }
        }
    }

    // ✅ API Response DTOs
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class OrderItemApiResponse
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class ProductApiResponse
    {
        public Guid Id { get; set; }
        public string? ProductName { get; set; }
        public string? SKU { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
    }

    public class VariantApiResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? VariantName { get; set; }
        public string? SKU { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}