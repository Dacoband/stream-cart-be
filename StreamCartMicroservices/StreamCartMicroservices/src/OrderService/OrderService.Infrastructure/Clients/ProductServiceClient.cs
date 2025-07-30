using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces.IServices;

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
                
                var response = await _httpClient.GetAsync($"/api/products/{productId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Product with ID: {ProductId} not found or error occurred. Status code: {StatusCode}", 
                        productId, response.StatusCode);
                    return null;
                }
                
                var product = await response.Content.ReadFromJsonAsync<ProductDto>();
                return product;
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
                
                var response = await _httpClient.GetAsync($"/api/product-variants/{variantId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Variant with ID: {VariantId} not found or error occurred. Status code: {StatusCode}", 
                        variantId, response.StatusCode);
                    return null;
                }
                
                var variant = await response.Content.ReadFromJsonAsync<VariantDto>();
                return variant;
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
                    ProductId = productId,
                    QuantityChange = quantity
                };
                
                var response = await _httpClient.PostAsJsonAsync("/api/products/stock/update", updateStockData);
                
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
                    VariantId = variantId,
                    QuantityChange = quantity
                };
                
                var response = await _httpClient.PostAsJsonAsync("/api/products/variants/stock/update", updateStockData);
                
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
    }
}