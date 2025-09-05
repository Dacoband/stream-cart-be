using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces.IServices;
using Shared.Common.Models;

namespace OrderService.Infrastructure.Clients
{
    /// <summary>
    /// Implementation of the IProductServiceClient interface
    /// </summary>
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductServiceClient> _logger;

        /// <summary>
        /// Creates a new instance of ProductServiceClient
        /// </summary>
        /// <param name="httpClient">HTTP client</param>
        /// <param name="logger">Logger</param>
        public ProductServiceClient(HttpClient httpClient, ILogger<ProductServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets product details by ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Product details if found, null otherwise</returns>
        public async Task<ProductDto> GetProductByIdAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Getting product details for ID: {ProductId}", productId);
                
                var response = await _httpClient.GetAsync($"https://brightpa.me/api/products/{productId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Product with ID: {ProductId} not found or error occurred. Status code: {StatusCode}", 
                        productId, response.StatusCode);
                    return null;
                }
                
                var product = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
                return product.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product details for ID: {ProductId}", productId);
                return null;
            }
        }
        
        /// <summary>
        /// Gets variant details by ID
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        /// <returns>Variant details if found, null otherwise</returns>
        public async Task<VariantDto> GetVariantByIdAsync(Guid variantId)
        {
            try
            {
                _logger.LogInformation("Getting variant details for ID: {VariantId}", variantId);
                
                var response = await _httpClient.GetAsync($"https://brightpa.me/api/product-variants/{variantId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Variant with ID: {VariantId} not found or error occurred. Status code: {StatusCode}", 
                        variantId, response.StatusCode);
                    return null;
                }
                
                var variant = await response.Content.ReadFromJsonAsync<ApiResponse< VariantDto>>();
                return variant.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting variant details for ID: {VariantId}", variantId);
                return null;
            }
        }

        /// <summary>
        /// Updates product stock quantity
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="quantity">Quantity change (negative for decrease)</param>
        /// <returns>True if successful</returns>
        public async Task<bool> UpdateProductStockAsync(Guid productId, int quantity)
        {
            try
            {
                _logger.LogInformation("Updating stock for product ID: {ProductId} by quantity: {Quantity}", productId, quantity);

                var updateStockData = new
                {
                    QuantityChange = quantity  
                };

                var response = await _httpClient.PutAsJsonAsync($"api/products/{productId}/stock", updateStockData);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update stock for product ID: {ProductId}. Status code: {StatusCode}",
                        productId, response.StatusCode);
                    return false;
                }

                _logger.LogInformation("Successfully updated stock for product ID: {ProductId}", productId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for product ID: {ProductId}", productId);
                return false;
            }
        }

        /// <summary>
        /// Updates variant stock quantity
        /// </summary>
        /// <param name="variantId">Variant ID</param>
        /// <param name="quantity">Quantity change (negative for decrease)</param>
        /// <returns>True if successful</returns>
        public async Task<bool> UpdateVariantStockAsync(Guid variantId, int quantity)
        {
            try
            {
                _logger.LogInformation("Updating stock for variant ID: {VariantId} by quantity: {Quantity}", variantId, quantity);

                var updateStockData = new
                {
                    Quantity = quantity
                };

                var response = await _httpClient.PatchAsJsonAsync($"https://brightpa.me/api/product-variants/{variantId}/stock", updateStockData);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to update stock for variant ID: {VariantId}. Status code: {StatusCode}",
                        variantId, response.StatusCode);
                    return false;
                }

                _logger.LogInformation("Successfully updated stock for variant ID: {VariantId}", variantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for variant ID: {VariantId}", variantId);
                return false;
            }
        }
        public async Task<bool> UpdateProductQuantitySoldAsync(Guid productId, int quantityChange)
        {
            try
            {
                var request = new
                {
                    QuantityChange = quantityChange,
                    UpdatedBy = "OrderService"
                };

                var response = await _httpClient.PutAsJsonAsync(
                    $"https://brightpa.me/api/products/{productId}/quantity-sold",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Updated QuantitySold for product {ProductId} by {Quantity}",
                        productId, quantityChange);
                    return true;
                }
                else
                {
                    _logger.LogWarning("❌ Failed to update QuantitySold for product {ProductId}: {StatusCode}",
                        productId, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error updating QuantitySold for product {ProductId}", productId);
                return false;
            }
        }
        public async Task<List<FlashSaleDetailDTO>> GetCurrentFlashSalesAsync()
        {
            try
            {
                _logger.LogInformation("Getting current flash sales...");

                var resp = await _httpClient.GetAsync("https://brightpa.me/api/flashsales/current");
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get current flash sales. Status: {StatusCode}", resp.StatusCode);
                    return new List<FlashSaleDetailDTO>();
                }

                var payload = await resp.Content.ReadFromJsonAsync<ApiResponse<List<FlashSaleDetailDTO>>>();
                if (payload == null)
                {
                    _logger.LogWarning("Empty body when getting current flash sales.");
                    return new List<FlashSaleDetailDTO>();
                }

                if (!payload.Success)
                {
                    _logger.LogWarning("Get current flash sales unsuccessful: {Message}", payload.Message);
                    return new List<FlashSaleDetailDTO>();
                }

                return payload.Data ?? new List<FlashSaleDetailDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current flash sales");
                return new List<FlashSaleDetailDTO>();
            }
        }

        /// <summary>
        /// PATCH /api/flashsales/{id}/sold  (body: int quantity)
        /// </summary>
        public async Task<bool> IncreaseFlashSaleSoldAsync(Guid flashSaleId, int quantity)
        {
            try
            {
                _logger.LogInformation("Increasing flash sale sold: {FlashSaleId} by {Quantity}", flashSaleId, quantity);

                // Body là số nguyên (JSON number)
                var resp = await _httpClient.PatchAsJsonAsync(
                    $"https://brightpa.me/api/flashsales/{flashSaleId}/sold",
                    quantity
                );

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to increase flash sale sold. Status: {StatusCode}", resp.StatusCode);
                    return false;
                }

                // Nếu muốn đọc dữ liệu trả về:
                // var payload = await resp.Content.ReadFromJsonAsync<ApiResponse<FlashSaleDetailDto>>();
                // return payload?.Success == true;

                _logger.LogInformation("Successfully increased flash sale sold for {FlashSaleId}", flashSaleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error increasing flash sale sold for {FlashSaleId}", flashSaleId);
                return false;
            }
        }

    }
}