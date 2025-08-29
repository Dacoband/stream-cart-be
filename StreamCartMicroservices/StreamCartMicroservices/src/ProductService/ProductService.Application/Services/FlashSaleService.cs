using Appwrite.Models;
using ProductService.Application.DTOs.Combinations;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Helpers;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class FlashSaleService : IFlashSaleService
    {
        private readonly IFlashSaleRepository _flashSaleRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IProductImageRepository _productImageRepository;

        public FlashSaleService(IFlashSaleRepository flashSaleRepository, IProductRepository productRepository, IProductVariantRepository productVariantRepository, IHttpClientFactory httpClientFactory, IProductImageRepository productImageRepository)
        {
            _flashSaleRepository = flashSaleRepository;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _httpClientFactory = httpClientFactory;
            _productImageRepository = productImageRepository;
        }

        public async Task<ApiResponse<List<int>>> GetAvailableSlotsAsync(DateTime date)
        {
            var response = new ApiResponse<List<int>>
            {
                Success = true,
                Message = "Lấy danh sách slot khả dụng thành công"
            };

            try
            {
                var availableSlots = await _flashSaleRepository.GetAvailableSlotsAsync(date.Date);
                response.Data = availableSlots;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lấy danh sách slot khả dụng: " + ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse<List<DetailFlashSaleDTO>>> CreateFlashSale(CreateFlashSaleDTO request, string userId, string shopId)
        {
            var response = new ApiResponse<List<DetailFlashSaleDTO>>()
            {
                Success = true,
                Message = "Tạo FlashSale thành công",
                Data = new List<DetailFlashSaleDTO>()
            };

            var errorMessages = new List<string>();

            // Validate slot
            if (!FlashSaleSlotHelper.SlotTimeRanges.ContainsKey(request.Slot))
            {
                response.Success = false;
                response.Message = $"Slot {request.Slot} không hợp lệ. Slot hợp lệ từ 1-7";
                return response;
            }

            // Lấy thời gian start/end theo slot và ngày
            var slotTime = FlashSaleSlotHelper.GetSlotTimeForDate(request.Slot, request.Date.Date);
            var startTime = slotTime.Start;
            var endTime = slotTime.End;

            // ✅ FIX: Sử dụng startTime và endTime thay vì chỉ date
            var isSlotAvailable = await _flashSaleRepository.IsSlotAvailableAsync(request.Slot, startTime, endTime);
            if (!isSlotAvailable)
            {
                response.Success = false;
                response.Message = $"Slot {request.Slot} đã bị sử dụng trong ngày {request.Date:dd/MM/yyyy}";
                return response;
            }

            foreach (var productRequest in request.Products)
            {
                var existingProduct = await _productRepository.GetByIdAsync(productRequest.ProductId.ToString());
                if (existingProduct == null || existingProduct.IsActive == false || existingProduct.IsDeleted == true)
                {
                    errorMessages.Add($"Không tìm thấy sản phẩm {productRequest.ProductId} cần áp dụng FlashSale");
                    continue;
                }

                if (existingProduct.ShopId.ToString() != shopId)
                {
                    errorMessages.Add($"Bạn không có quyền tạo FlashSale cho sản phẩm {productRequest.ProductId}");
                    continue;
                }

                if (productRequest.VariantIds == null || !productRequest.VariantIds.Any() || productRequest.VariantIds.All(v => v == null))
                {
                    await CreateFlashSaleForProduct(
                        productRequest.ProductId,
                        null,
                        productRequest.FlashSalePrice,
                        productRequest.QuantityAvailable ?? request.QuantityAvailable,
                        startTime,
                        endTime,
                        request.Slot,
                        userId,
                        response.Data,
                        errorMessages);
                }
                else
                {
                    // Create FlashSale for each variant
                    foreach (var variantId in productRequest.VariantIds)
                    {
                        await CreateFlashSaleForProduct(
                            productRequest.ProductId,
                            variantId,
                            productRequest.FlashSalePrice,
                            productRequest.QuantityAvailable ?? request.QuantityAvailable,
                            startTime,
                            endTime,
                            request.Slot,
                            userId,
                            response.Data,
                            errorMessages);
                    }
                }
            }

            if (!response.Data.Any())
            {
                response.Success = false;
                response.Message = "Không có sản phẩm nào hợp lệ để áp dụng FlashSale:\n" + string.Join("\n", errorMessages);
                return response;
            }

            if (errorMessages.Any())
            {
                response.Message += $" Tuy nhiên, một số sản phẩm không áp dụng được:\n{string.Join("\n", errorMessages)}";
            }

            return response;
        }

        private async Task CreateFlashSaleForProduct(Guid productId, Guid? variantId, decimal flashSalePrice,
            int? quantityAvailable, DateTime startTime, DateTime endTime, int slot, string userId,
            List<DetailFlashSaleDTO> results, List<string> errorMessages)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId.ToString());

                int maxStock;
                decimal maxPrice;
                string productName = product.ProductName;
                string variantName = "";

                if (variantId.HasValue)
                {
                    var variant = await _productVariantRepository.GetByIdAsync(variantId.ToString());
                    if (variant == null || variant.IsDeleted)
                    {
                        errorMessages.Add($"Variant {variantId} không tồn tại");
                        return;
                    }
                    maxStock = variant.Stock;
                    maxPrice = variant.Price;
                    variantName = await GetVariantNameAsync(variantId.Value) ?? variant.SKU ?? $"Variant {variantId}";
                }
                else
                {
                    maxStock = product.StockQuantity;
                    maxPrice = product.BasePrice;
                    variantName = "";
                }

                var finalQuantityAvailable = quantityAvailable ?? maxStock;

                if (finalQuantityAvailable > maxStock)
                {
                    errorMessages.Add($"Sản phẩm {productName} không đủ tồn kho để áp dụng FlashSale (yêu cầu: {finalQuantityAvailable}, có: {maxStock})");
                    return;
                }

                if (flashSalePrice >= maxPrice)
                {
                    errorMessages.Add($"Giá FlashSale ({flashSalePrice:N0}đ) phải thấp hơn giá gốc ({maxPrice:N0}đ) của {productName}");
                    return;
                }

                var existingFlashSales = await _flashSaleRepository.GetByTimeAndProduct(startTime, endTime, productId, variantId);
                var activeConflictingSales = existingFlashSales.Where(fs => !fs.IsDeleted).ToList();

                if (activeConflictingSales.Any())
                {
                    errorMessages.Add($"Sản phẩm {productName} đã có FlashSale trong khoảng thời gian trùng lặp");
                    return;
                }

                var flashSale = new FlashSale()
                {
                    ProductId = productId,
                    VariantId = variantId,
                    FlashSalePrice = flashSalePrice,
                    QuantityAvailable = finalQuantityAvailable,
                    QuantitySold = 0,
                    StartTime = startTime,
                    EndTime = endTime,
                    Slot = slot
                };
                flashSale.SetCreator(userId);

                await _flashSaleRepository.InsertAsync(flashSale);

                string? productImageUrl = null;
                try
                {
                    var primaryImage = await _productImageRepository.GetPrimaryImageAsync(productId, variantId);
                    productImageUrl = primaryImage?.ImageUrl;
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the entire operation
                }

                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var result = new DetailFlashSaleDTO()
                {
                    Id = flashSale.Id,
                    ProductId = productId,
                    VariantId = variantId,
                    FlashSalePrice = flashSale.FlashSalePrice,
                    QuantityAvailable = flashSale.QuantityAvailable,
                    QuantitySold = 0,
                    StartTime = TimeZoneInfo.ConvertTimeFromUtc(flashSale.StartTime, tz),
                    EndTime = TimeZoneInfo.ConvertTimeFromUtc(flashSale.EndTime, tz),
                    Slot = flashSale.Slot,
                    IsActive = true,
                    ProductName = productName,
                    VariantName = variantName,
                    ProductImageUrl = productImageUrl
                };
                results.Add(result);
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Lỗi khi tạo FlashSale cho sản phẩm {productId}: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductWithoutFlashSaleDTO>>> GetProductsWithoutFlashSaleAsync(string shopId, DateTime date)
        {
            var response = new ApiResponse<List<ProductWithoutFlashSaleDTO>>
            {
                Success = true,
                Message = "Lấy danh sách sản phẩm không có FlashSale thành công"
            };

            try
            {
                if (!Guid.TryParse(shopId, out Guid shopGuid))
                {
                    response.Success = false;
                    response.Message = "Shop ID không hợp lệ";
                    return response;
                }

                // ✅ FIX: Sử dụng startTime và endTime thay vì chỉ date
                var startOfDay = date.Date;
                var endOfDay = date.Date.AddDays(1).AddTicks(-1);
                var productIdsWithoutFlashSale = await _flashSaleRepository.GetProductsWithoutFlashSaleAsync(shopGuid, startOfDay, endOfDay);
                var products = new List<ProductWithoutFlashSaleDTO>();

                foreach (var productId in productIdsWithoutFlashSale)
                {
                    var product = await _productRepository.GetByIdAsync(productId.ToString());
                    if (product == null) continue;

                    // Get product image
                    string? productImageUrl = null;
                    try
                    {
                        var primaryImage = await _productImageRepository.GetPrimaryImageAsync(productId, null);
                        productImageUrl = primaryImage?.ImageUrl;
                    }
                    catch (Exception) { }

                    // Get variants
                    var variants = new List<ProductVariantWithoutFlashSaleDTO>();
                    try
                    {
                        var productVariants = await _productVariantRepository.GetByProductIdAsync(productId);
                        foreach (var variant in productVariants.Where(v => !v.IsDeleted))
                        {
                            var variantName = await GetVariantNameAsync(variant.Id) ?? variant.SKU ?? $"Variant {variant.Id}";
                            variants.Add(new ProductVariantWithoutFlashSaleDTO
                            {
                                Id = variant.Id,
                                SKU = variant.SKU,
                                Price = variant.Price,
                                Stock = variant.Stock,
                                VariantName = variantName
                            });
                        }
                    }
                    catch (Exception) { }

                    products.Add(new ProductWithoutFlashSaleDTO
                    {
                        Id = product.Id,
                        ProductName = product.ProductName,
                        Description = product.Description,
                        SKU = product.SKU,
                        BasePrice = product.BasePrice,
                        StockQuantity = product.StockQuantity,
                        ProductImageUrl = productImageUrl,
                        Variants = variants.Any() ? variants : null
                    });
                }

                response.Data = products;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lấy danh sách sản phẩm: " + ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse<List<DetailFlashSaleDTO>>> GetFlashSalesByShopIdAsync(string shopId, FilterFlashSaleDTO filter)
        {
            var response = new ApiResponse<List<DetailFlashSaleDTO>>
            {
                Success = true,
                Message = "Lấy danh sách FlashSale của shop thành công",
                Data = new List<DetailFlashSaleDTO>()
            };

            try
            {
                if (!Guid.TryParse(shopId, out Guid shopGuid))
                {
                    response.Success = false;
                    response.Message = "Shop ID không hợp lệ";
                    return response;
                }

                var shopFlashSales = await _flashSaleRepository.GetByShopIdAsync(shopGuid);
                var query = shopFlashSales.AsQueryable();

                var nowUtc = DateTime.UtcNow;

                if (filter.IsActive.HasValue)
                {
                    if (filter.IsActive.Value)
                    {
                        query = query.Where(f => !f.IsDeleted &&
                                                f.StartTime <= nowUtc &&
                                                f.EndTime >= nowUtc);
                    }
                    else
                    {
                        query = query.Where(f => f.IsDeleted ||
                                                f.StartTime > nowUtc ||
                                                f.EndTime < nowUtc);
                    }
                }

                if (filter.Slot.HasValue)
                {
                    query = query.Where(f => f.Slot == filter.Slot.Value);
                }

                if (filter.ProductId != null && filter.ProductId.Any())
                {
                    query = query.Where(f => filter.ProductId.Contains(f.ProductId));
                }

                if (filter.VariantId != null && filter.VariantId.Any())
                {
                    query = query.Where(f => f.VariantId.HasValue && filter.VariantId.Contains(f.VariantId.Value));
                }

                if (filter.StartDate.HasValue)
                {
                    var startUtc = filter.StartDate.Value.Kind == DateTimeKind.Utc
                        ? filter.StartDate.Value
                        : TimeZoneInfo.ConvertTimeToUtc(filter.StartDate.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    query = query.Where(f => f.EndTime >= startUtc);
                }

                if (filter.EndDate.HasValue)
                {
                    var endUtc = filter.EndDate.Value.Kind == DateTimeKind.Utc
                        ? filter.EndDate.Value
                        : TimeZoneInfo.ConvertTimeToUtc(filter.EndDate.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    query = query.Where(f => f.StartTime <= endUtc);
                }

                switch (filter.OrderBy)
                {
                    case FlashSaleOrderBy.EndDate:
                        query = filter.OrderDirection == OrderDirection.Desc
                            ? query.OrderByDescending(f => f.EndTime)
                            : query.OrderBy(f => f.EndTime);
                        break;
                    case FlashSaleOrderBy.Slot:
                        query = filter.OrderDirection == OrderDirection.Desc
                            ? query.OrderByDescending(f => f.Slot)
                            : query.OrderBy(f => f.Slot);
                        break;
                    case FlashSaleOrderBy.StartDate:
                    default:
                        query = filter.OrderDirection == OrderDirection.Desc
                            ? query.OrderByDescending(f => f.StartTime)
                            : query.OrderBy(f => f.StartTime);
                        break;
                }

                // Fix pagination issue - đây là điểm quan trọng
                var pageIndex = filter.PageIndex ?? 0;
                var pageSize = filter.PageSize ?? 10;

                // Đảm bảo pageIndex không âm và pageSize hợp lệ
                if (pageIndex < 0) pageIndex = 0;
                if (pageSize <= 0) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Giới hạn tối đa

                var totalCount = query.Count();
                query = query.Skip(pageIndex * pageSize).Take(pageSize);

                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var flashSaleList = query.ToList();
                var detailFlashSales = new List<DetailFlashSaleDTO>();

                foreach (var f in flashSaleList)
                {
                    string? productImageUrl = null;
                    string productName = "Unknown Product";
                    string variantName = "";

                    try
                    {
                        var product = await _productRepository.GetByIdAsync(f.ProductId.ToString());
                        productName = product?.ProductName ?? productName;

                        var primaryImage = await _productImageRepository.GetPrimaryImageAsync(f.ProductId, f.VariantId);
                        productImageUrl = primaryImage?.ImageUrl;

                        if (f.VariantId.HasValue)
                        {
                            variantName = await GetVariantNameAsync(f.VariantId.Value) ?? "";
                        }
                    }
                    catch (Exception)
                    {
                        // Silent fail
                    }

                    detailFlashSales.Add(new DetailFlashSaleDTO
                    {
                        Id = f.Id,
                        ProductId = f.ProductId,
                        VariantId = f.VariantId != Guid.Empty ? f.VariantId : null,
                        FlashSalePrice = f.FlashSalePrice,
                        QuantityAvailable = f.QuantityAvailable,
                        QuantitySold = f.QuantitySold,
                        Slot = f.Slot,
                        IsActive = !f.IsDeleted && f.StartTime <= nowUtc && f.EndTime >= nowUtc,
                        StartTime = TimeZoneInfo.ConvertTimeFromUtc(f.StartTime, tz),
                        EndTime = TimeZoneInfo.ConvertTimeFromUtc(f.EndTime, tz),
                        ProductName = productName,
                        VariantName = variantName,
                        ProductImageUrl = productImageUrl
                    });
                }

                response.Data = detailFlashSales;
                response.Message = $"Lấy danh sách FlashSale thành công. Trang {pageIndex + 1}/{Math.Ceiling((double)totalCount / pageSize)}, tổng {totalCount} bản ghi";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lấy danh sách FlashSale của shop: " + ex.Message;
                return response;
            }
        }

        // Giữ nguyên các method khác...
        private async Task<string?> GetVariantNameAsync(Guid variantId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync($"https://brightpa.me/api/product-combinations/{variantId}");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProductCombinationDto>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Success == true && apiResponse.Data != null && apiResponse.Data.Any())
                {
                    var parts = apiResponse.Data
                        .Select(d => $"{d.AttributeName} {d.ValueName}")
                        .ToArray();
                    return string.Join(" , ", parts);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ApiResponse<bool>> DeleteFlashsale(string id, string userId, string shopId)
        {
            var response = new ApiResponse<bool>()
            {
                Success = true,
                Message = "Xóa FlashSale thành công",
            };

            var existingFlashSale = await _flashSaleRepository.GetByIdAsync(id);
            if (existingFlashSale == null || existingFlashSale.IsDeleted == true)
            {
                response.Success = false;
                response.Message = "Không tìm thấy FlashSale";
                return response;
            }

            var existingProduct = await _productRepository.GetByIdAsync(existingFlashSale.ProductId.ToString());
            if (existingProduct == null || existingProduct.ShopId.ToString() != shopId)
            {
                response.Success = false;
                response.Message = "Bạn không có quyền xóa Flashsale này";
                return response;
            }
            var now = DateTime.UtcNow;
            if (existingFlashSale.StartTime <= now && existingFlashSale.EndTime >= now)
            {
                response.Success = false;
                response.Message = "Không thể xóa FlashSale đang diễn ra";
                return response;
            }

            try
            {
                existingFlashSale.Delete(userId);
                await _flashSaleRepository.ReplaceAsync(existingFlashSale.Id.ToString(), existingFlashSale);
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Xảy ra lỗi khi xóa Flashsale";
                return response;
            }
        }

        public async Task<ApiResponse<List<DetailFlashSaleDTO>>> FilterFlashSale(FilterFlashSaleDTO filter)
        {
            var response = new ApiResponse<List<DetailFlashSaleDTO>>
            {
                Success = true,
                Message = "Lọc danh sách FlashSale thành công",
                Data = new List<DetailFlashSaleDTO>()
            };

            try
            {
                var allFlashSales = await _flashSaleRepository.GetAllAsync();
                var query = allFlashSales.AsQueryable();

                var nowUtc = DateTime.UtcNow;

                if (filter.IsActive.HasValue)
                {
                    if (filter.IsActive.Value)
                    {
                        query = query.Where(f => !f.IsDeleted &&
                                                f.StartTime <= nowUtc &&
                                                f.EndTime >= nowUtc);
                    }
                    else
                    {
                        query = query.Where(f => f.IsDeleted ||
                                                f.StartTime > nowUtc ||
                                                f.EndTime < nowUtc);
                    }
                }

                if (filter.ProductId != null && filter.ProductId.Any())
                {
                    query = query.Where(f => filter.ProductId.Contains(f.ProductId));
                }

                if (filter.VariantId != null && filter.VariantId.Any())
                {
                    query = query.Where(f => f.VariantId.HasValue && filter.VariantId.Contains(f.VariantId.Value));
                }

                if (filter.StartDate.HasValue)
                {
                    var startUtc = filter.StartDate.Value.Kind == DateTimeKind.Utc
                        ? filter.StartDate.Value
                        : TimeZoneInfo.ConvertTimeToUtc(filter.StartDate.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    query = query.Where(f => f.EndTime >= startUtc);
                }

                if (filter.EndDate.HasValue)
                {
                    var endUtc = filter.EndDate.Value.Kind == DateTimeKind.Utc
                        ? filter.EndDate.Value
                        : TimeZoneInfo.ConvertTimeToUtc(filter.EndDate.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    query = query.Where(f => f.StartTime <= endUtc);
                }

                switch (filter.OrderBy)
                {
                    case FlashSaleOrderBy.EndDate:
                        query = filter.OrderDirection == OrderDirection.Desc
                            ? query.OrderByDescending(f => f.EndTime)
                            : query.OrderBy(f => f.EndTime);
                        break;

                    case FlashSaleOrderBy.StartDate:
                    default:
                        query = filter.OrderDirection == OrderDirection.Desc
                            ? query.OrderByDescending(f => f.StartTime)
                            : query.OrderBy(f => f.StartTime);
                        break;
                }

                var pageIndex = filter.PageIndex ?? 0;
                var pageSize = filter.PageSize ?? 10;
                query = query.Skip(pageIndex * pageSize).Take(pageSize);
                var flashSaleList = query.ToList();
                var detailFlashSales = new List<DetailFlashSaleDTO>();

                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                foreach (var f in flashSaleList)
                {
                    string? productImageUrl = null;
                    string productName = "Unknown Product";

                    try
                    {
                        var product = await _productRepository.GetByIdAsync(f.ProductId.ToString());
                        productName = product?.ProductName ?? productName;

                        var primaryImage = await _productImageRepository.GetPrimaryImageAsync(f.ProductId, f.VariantId);
                        productImageUrl = primaryImage?.ImageUrl;
                    }
                    catch (Exception)
                    {
                    }

                    detailFlashSales.Add(new DetailFlashSaleDTO
                    {
                        Id = f.Id,
                        ProductId = f.ProductId,
                        VariantId = f.VariantId != Guid.Empty ? f.VariantId : null,
                        FlashSalePrice = f.FlashSalePrice,
                        QuantityAvailable = f.QuantityAvailable,
                        QuantitySold = f.QuantitySold,
                        IsActive = !f.IsDeleted && f.StartTime <= nowUtc && f.EndTime >= nowUtc,
                        StartTime = TimeZoneInfo.ConvertTimeFromUtc(f.StartTime, tz),
                        EndTime = TimeZoneInfo.ConvertTimeFromUtc(f.EndTime, tz),
                        ProductName = productName,
                        ProductImageUrl = productImageUrl
                    });
                }

                response.Data = detailFlashSales;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lọc FlashSale: " + ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse<List<DetailFlashSaleDTO>>> GetFlashSalesByShopAndDateAsync(string shopId, DateTime? date, int? slot)
        {
            var response = new ApiResponse<List<DetailFlashSaleDTO>>
            {
                Success = true,
                Message = "Lấy danh sách FlashSale của shop thành công",
                Data = new List<DetailFlashSaleDTO>()
            };

            try
            {
                if (!Guid.TryParse(shopId, out Guid shopGuid))
                {
                    response.Success = false;
                    response.Message = "Shop ID không hợp lệ";
                    return response;
                }

                var shopFlashSales = await _flashSaleRepository.GetByShopIdAsync(shopGuid);
                var query = shopFlashSales.AsQueryable();

                if (date.HasValue)
                {
                    var startOfDay = date.Value.Date;
                    var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

                    query = query.Where(f => f.StartTime >= startOfDay && f.StartTime <= endOfDay);
                }

                if (slot.HasValue)
                {
                    query = query.Where(f => f.Slot == slot.Value);
                }
                query = query.OrderByDescending(f => f.CreatedAt)
                           .ThenByDescending(f => f.StartTime);

                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                var flashSaleList = query.ToList();
                var detailFlashSales = new List<DetailFlashSaleDTO>();

                foreach (var f in flashSaleList)
                {
                    string? productImageUrl = null;
                    string productName = "Unknown Product";

                    try
                    {
                        var product = await _productRepository.GetByIdAsync(f.ProductId.ToString());
                        productName = product?.ProductName ?? productName;

                        var primaryImage = await _productImageRepository.GetPrimaryImageAsync(f.ProductId, f.VariantId);
                        productImageUrl = primaryImage?.ImageUrl;
                    }
                    catch (Exception)
                    {
                        // Silent fail
                    }

                    detailFlashSales.Add(new DetailFlashSaleDTO
                    {
                        Id = f.Id,
                        ProductId = f.ProductId,
                        VariantId = f.VariantId,
                        FlashSalePrice = f.FlashSalePrice,
                        QuantityAvailable = f.QuantityAvailable,
                        QuantitySold = f.QuantitySold,
                        Slot = f.Slot,
                        IsActive = f.IsValid(),
                        StartTime = TimeZoneInfo.ConvertTimeFromUtc(f.StartTime, tz),
                        EndTime = TimeZoneInfo.ConvertTimeFromUtc(f.EndTime, tz),
                        ProductName = productName,
                        ProductImageUrl = productImageUrl
                    });
                }
                response.Data = detailFlashSales;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lấy danh sách FlashSale của shop: " + ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse<DetailFlashSaleDTO>> GetFlashSaleById(string id)
        {
            var response = new ApiResponse<DetailFlashSaleDTO>
            {
                Success = true,
                Message = "Lấy FlashSale thành công",
            };

            var flashSale = await _flashSaleRepository.GetByIdAsync(id);
            if (flashSale == null)
            {
                response.Success = false;
                response.Message = "Không tìm thấy FlashSale";
                return response;
            }

            var product = await _productRepository.GetByIdAsync(flashSale.ProductId.ToString());
            string? productImageUrl = null;
            string productName = product?.ProductName ?? "Unknown Product";
            string variantName = "";

            try
            {
                var primaryImage = await _productImageRepository.GetPrimaryImageAsync(flashSale.ProductId, flashSale.VariantId);
                productImageUrl = primaryImage?.ImageUrl;

                if (flashSale.VariantId.HasValue)
                {
                    variantName = await GetVariantNameAsync(flashSale.VariantId.Value) ?? "";
                }
            }
            catch (Exception)
            {
                // Silent fail for image/variant name
            }
            var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            DetailFlashSaleDTO result = new DetailFlashSaleDTO()
            {
                Id = flashSale.Id,
                ProductId = flashSale.ProductId,
                VariantId = flashSale.VariantId,
                FlashSalePrice = flashSale.FlashSalePrice,
                QuantityAvailable = flashSale.QuantityAvailable,
                StartTime = TimeZoneInfo.ConvertTimeFromUtc(flashSale.StartTime, tz),
                EndTime = TimeZoneInfo.ConvertTimeFromUtc(flashSale.EndTime, tz),
                Slot = flashSale.Slot,
                IsActive = flashSale.IsValid(),
                QuantitySold = flashSale.QuantitySold,
                ProductName = productName,
                VariantName = variantName,
                ProductImageUrl = productImageUrl
            };
            response.Data = result;
            return response;
        }

        public async Task<ApiResponse<DetailFlashSaleDTO>> UpdateFlashSale(UpdateFlashSaleDTO request, string flashSaleId, string userId, string shopId)
        {
            var response = new ApiResponse<DetailFlashSaleDTO>()
            {
                Success = true,
                Message = "Cập nhật FlashSale thành công",
            };

            var existingFlashSale = await _flashSaleRepository.GetByIdAsync(flashSaleId);
            if (existingFlashSale == null || existingFlashSale.IsDeleted == true)
            {
                response.Success = false;
                response.Message = "Không tìm thấy FlashSale";
                return response;
            }

            var existingProduct = await _productRepository.GetByIdAsync(existingFlashSale.ProductId.ToString());
            if (existingProduct == null || existingProduct.ShopId.ToString() != shopId)
            {
                response.Success = false;
                response.Message = "Bạn không có quyền cập nhật FlashSale cho sản phẩm này";
                return response;
            }

            // Không cho phép cập nhật nếu đang diễn ra
            var now = DateTime.UtcNow;
            if (existingFlashSale.StartTime <= now && existingFlashSale.EndTime >= now)
            {
                response.Success = false;
                response.Message = "Không thể cập nhật FlashSale đang diễn ra";
                return response;
            }

            // Kiểm tra chồng thời gian FlashSale (bỏ qua chính nó)
            var newStart = request.StartTime ?? existingFlashSale.StartTime;
            var newEnd = request.EndTime ?? existingFlashSale.EndTime;
            var overlapFlashSales = await _flashSaleRepository.GetByTimeAndProduct(newStart, newEnd, existingFlashSale.ProductId, existingFlashSale.VariantId);

            var hasConflict = overlapFlashSales.Any(fs => fs.Id != existingFlashSale.Id && !fs.IsDeleted);
            if (hasConflict)
            {
                response.Success = false;
                response.Message = "Chỉ có thể áp dụng 1 FlashSale cho cùng 1 thời điểm";
                return response;
            }

            // Validate giá và số lượng
            var maxStock = Guid.Empty.Equals(existingFlashSale.VariantId)
                ? existingProduct.StockQuantity
                : (await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString()))?.Stock ?? 0;

            if (request.QuantityAvailable.HasValue && request.QuantityAvailable.Value > maxStock)
            {
                response.Success = false;
                response.Message = "Không đủ số lượng sản phẩm tồn kho để áp dụng FlashSale";
                return response;
            }

            var maxPrice = Guid.Empty.Equals(existingFlashSale.VariantId)
                ? existingProduct.BasePrice
                : (await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString()))?.Price ?? 0;

            if (request.FLashSalePrice.HasValue && request.FLashSalePrice.Value >= maxPrice)
            {
                response.Success = false;
                response.Message = "Giá FlashSale phải thấp hơn giá sản phẩm";
                return response;
            }

            if (request.FLashSalePrice.HasValue)
                existingFlashSale.FlashSalePrice = request.FLashSalePrice.Value;

            if (request.QuantityAvailable.HasValue)
                existingFlashSale.QuantityAvailable = request.QuantityAvailable.Value;

            if (request.StartTime.HasValue)
                existingFlashSale.StartTime = request.StartTime.Value;

            if (request.EndTime.HasValue)
                existingFlashSale.EndTime = request.EndTime.Value;

            existingFlashSale.SetModifier(userId);

            try
            {
                await _flashSaleRepository.ReplaceAsync(existingFlashSale.Id.ToString(), existingFlashSale);

                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var result = new DetailFlashSaleDTO()
                {
                    Id = existingFlashSale.Id,
                    ProductId = existingFlashSale.ProductId,
                    VariantId = existingFlashSale.VariantId,
                    FlashSalePrice = existingFlashSale.FlashSalePrice,
                    QuantityAvailable = existingFlashSale.QuantityAvailable,
                    QuantitySold = existingFlashSale.QuantitySold,
                    StartTime = TimeZoneInfo.ConvertTimeFromUtc(existingFlashSale.StartTime, tz),
                    EndTime = TimeZoneInfo.ConvertTimeFromUtc(existingFlashSale.EndTime, tz),
                    Slot = existingFlashSale.Slot,
                    IsActive = existingFlashSale.IsValid(),
                };
                response.Data = result;
                return response;
            }
            catch (Exception)
            {
                response.Success = false;
                response.Message = "Lỗi khi cập nhật FlashSale";
                return response;
            }
        }

        public async Task<ApiResponse<bool>> UpdateFlashSaleProductsAsync(string flashSaleId, List<Guid> productIds, List<Guid>? variantIds, string userId, string shopId)
        {
            var response = new ApiResponse<bool>
            {
                Success = true,
                Message = "Cập nhật sản phẩm FlashSale thành công"
            };

            try
            {
                var existingFlashSale = await _flashSaleRepository.GetByIdAsync(flashSaleId);
                if (existingFlashSale == null || existingFlashSale.IsDeleted)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy FlashSale";
                    return response;
                }

                // Check if FlashSale is currently active
                var now = DateTime.UtcNow;
                if (existingFlashSale.StartTime <= now && existingFlashSale.EndTime >= now)
                {
                    response.Success = false;
                    response.Message = "Không thể cập nhật FlashSale đang diễn ra";
                    return response;
                }

                // Validate shop ownership for all products
                foreach (var productId in productIds)
                {
                    var product = await _productRepository.GetByIdAsync(productId.ToString());
                    if (product == null || product.ShopId.ToString() != shopId)
                    {
                        response.Success = false;
                        response.Message = $"Bạn không có quyền cập nhật sản phẩm {productId}";
                        return response;
                    }
                }
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi cập nhật sản phẩm FlashSale: " + ex.Message;
                return response;
            }
        }
    }
}