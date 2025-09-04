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
                Message = "Lấy danh sách slot khả dụng thành công",
                Data = new List<int>()
            };

            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var nowVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

                DateTime targetDateVn;

                if (date.Kind == DateTimeKind.Utc)
                {
                    targetDateVn = TimeZoneInfo.ConvertTimeFromUtc(date, tz).Date;
                }
                else
                {
                    targetDateVn = date.Date;
                }

                var startOfDayVn = targetDateVn; 
                var endOfDayVn = targetDateVn.AddDays(1).AddTicks(-1); 

                var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayVn, tz);
                var endOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(endOfDayVn, tz);

                var availableSlots = await _flashSaleRepository.GetAvailableSlotsAsync(startOfDayUtc, endOfDayUtc);

                if (targetDateVn.Date == nowVn.Date)
                {
                    var validSlots = new List<int>();
                    foreach (var slot in availableSlots)
                    {
                        if (FlashSaleSlotHelper.SlotTimeRanges.ContainsKey(slot))
                        {
                            var slotTimeRange = FlashSaleSlotHelper.SlotTimeRanges[slot];
                            var slotStartTimeVn = targetDateVn.Add(slotTimeRange.Start);
                            if (slotStartTimeVn > nowVn)
                            {
                                validSlots.Add(slot);
                            }
                        }
                    }
                    response.Data = validSlots;
                }
                else if (targetDateVn.Date > nowVn.Date)
                {
                    response.Data = availableSlots;
                }
                else
                {
                    response.Data = new List<int>();
                }

                response.Message = $"Lấy danh sách slot khả dụng thành công cho ngày {targetDateVn:yyyy-MM-dd}. " +
                                  $"Hiện tại: {nowVn:HH:mm dd/MM/yyyy} (VN)";
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
                response.Message = $"Slot {request.Slot} không hợp lệ. Slot hợp lệ từ 1-8";
                return response;
            }

            // Lấy thời gian start/end theo slot và ngày
            var slotTime = FlashSaleSlotHelper.GetSlotTimeForDate(request.Slot, request.Date.Date);
            var startTime = slotTime.Start;
            var endTime = slotTime.End;

            //var isSlotAvailable = await _flashSaleRepository.IsSlotAvailableAsync(request.Slot, startTime, endTime);
            //if (!isSlotAvailable)
            //{
            //    response.Success = false;
            //    response.Message = $"Slot {request.Slot} đã bị sử dụng trong ngày {request.Date:dd/MM/yyyy}";
            //    return response;
            //}
            var now = DateTime.UtcNow;
            if (startTime <= now)
            {
                response.Success = false;
                response.Message = $"Không thể tạo FlashSale cho slot {request.Slot} vì đã qua thời gian bắt đầu";
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
                decimal originalPrice;
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
                    originalPrice = variant.Price;
                    variantName = await GetVariantNameAsync(variantId.Value) ?? variant.SKU ?? $"Variant {variantId}";
                }
                else
                {
                    maxStock = product.StockQuantity;
                    originalPrice = product.BasePrice;
                    variantName = "";
                }

                var finalQuantityAvailable = quantityAvailable ?? maxStock;

                if (finalQuantityAvailable > maxStock)
                {
                    errorMessages.Add($"Sản phẩm {productName} không đủ tồn kho để áp dụng FlashSale (yêu cầu: {finalQuantityAvailable}, có: {maxStock})");
                    return;
                }

                if (flashSalePrice >= originalPrice)
                {
                    errorMessages.Add($"Giá FlashSale ({flashSalePrice:N0}đ) phải thấp hơn giá gốc ({originalPrice:N0}đ) của {productName}");
                    return;
                }

                var existingFlashSales = await _flashSaleRepository.GetByTimeAndProduct(startTime, endTime, productId, variantId);
                var activeConflictingSales = existingFlashSales.Where(fs => !fs.IsDeleted).ToList();

                if (activeConflictingSales.Any())
                {
                    errorMessages.Add($"Sản phẩm {productName} đã có FlashSale trong khoảng thời gian trùng lặp");
                    return;
                }
                bool reserveSuccess = false;
                if (variantId.HasValue)
                {
                    var variant = await _productVariantRepository.GetByIdAsync(variantId.ToString());
                    if (variant != null && variant.CanReserveStock(finalQuantityAvailable))
                    {
                        variant.ReserveStockForFlashSale(finalQuantityAvailable, userId);
                        await _productVariantRepository.ReplaceAsync(variant.Id.ToString(), variant);
                        reserveSuccess = true;
                    }
                    else
                    {
                        errorMessages.Add($"Variant {variantId} không đủ stock để reserve cho Flash Sale (cần: {finalQuantityAvailable}, có: {variant?.Stock ?? 0})");
                        return;
                    }
                }
                else
                {
                    reserveSuccess = product.AddReserveStock(finalQuantityAvailable);
                    if (reserveSuccess)
                    {
                        await _productRepository.ReplaceAsync(product.Id.ToString(), product);
                    }
                }

                if (!reserveSuccess)
                {
                    errorMessages.Add($"Không thể reserve stock cho sản phẩm {productName}");
                    return;
                }
                var flashSale = new FlashSale()
                {
                    ProductId = productId,
                    VariantId = variantId,
                    FlashSalePrice = flashSalePrice,
                    QuantityAvailable = finalQuantityAvailable,
                    QuantitySold = 0,
                    StartTime = startTime.Kind == DateTimeKind.Utc ? startTime : DateTime.SpecifyKind(startTime, DateTimeKind.Utc),
                    EndTime = endTime.Kind == DateTimeKind.Utc ? endTime : DateTime.SpecifyKind(endTime, DateTimeKind.Utc),
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
                    ProductImageUrl = productImageUrl,
                    Price = originalPrice,
                    Stock = maxStock
                };
                results.Add(result);
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Lỗi khi tạo FlashSale cho sản phẩm {productId}: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ProductWithoutFlashSaleDTO>>> GetProductsWithoutFlashSaleAsync(string shopId, DateTime date, int? slot = null)
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

                DateTime startTime, endTime;

                if (slot.HasValue)
                {
                    if (!FlashSaleSlotHelper.SlotTimeRanges.ContainsKey(slot.Value))
                    {
                        response.Success = false;
                        response.Message = $"Slot {slot.Value} không hợp lệ. Slot hợp lệ từ 1-8";
                        return response;
                    }

                    var slotTime = FlashSaleSlotHelper.GetSlotTimeForDate(slot.Value, date.Date);
                    startTime = slotTime.Start;
                    endTime = slotTime.End;
                }
                else
                {
                    startTime = date.Date;
                    endTime = date.Date.AddDays(1).AddTicks(-1);
                }
                var existingFlashSales = await _flashSaleRepository.GetAllAsync();
                var shopFlashSales = existingFlashSales
                    .Where(fs => !fs.IsDeleted &&
                                fs.StartTime < endTime && fs.EndTime > startTime) 
                    .ToList();

                var productIdsWithoutFlashSale = await _flashSaleRepository.GetProductsWithoutFlashSaleAsync(shopGuid, startTime, endTime);
                var products = new List<ProductWithoutFlashSaleDTO>();

                foreach (var productId in productIdsWithoutFlashSale)
                {
                    var product = await _productRepository.GetByIdAsync(productId.ToString());
                    if (product == null) continue;
                    string? productImageUrl = null;
                    try
                    {
                        var primaryImage = await _productImageRepository.GetPrimaryImageAsync(productId, null);
                        productImageUrl = primaryImage?.ImageUrl;
                    }
                    catch (Exception) { }
                    var variants = new List<ProductVariantWithoutFlashSaleDTO>();
                    bool hasAvailableVariants = false;
                    try
                    {
                        var productVariants = await _productVariantRepository.GetByProductIdAsync(productId);
                        foreach (var variant in productVariants.Where(v => !v.IsDeleted))
                        {
                            bool variantHasFlashSale = shopFlashSales.Any(fs =>
                                fs.ProductId == productId && fs.VariantId == variant.Id);

                            if (!variantHasFlashSale)
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
                                hasAvailableVariants = true;
                            }
                        }
                    }
                    catch (Exception) { }
                    bool productHasFlashSale = shopFlashSales.Any(fs =>
                         fs.ProductId == productId && fs.VariantId == null);
                    if (hasAvailableVariants || (!productHasFlashSale && !variants.Any()))
                    {
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
                }

                response.Data = products;

                if (slot.HasValue)
                {
                    var slotTimeRange = FlashSaleSlotHelper.SlotTimeRanges[slot.Value];
                    response.Message = $"Lấy danh sách sản phẩm không có FlashSale thành công cho ngày {date:dd/MM/yyyy} slot {slot.Value} ({slotTimeRange.Start:hh\\:mm}-{slotTimeRange.End:hh\\:mm})";
                }
                else
                {
                    response.Message = $"Lấy danh sách sản phẩm không có FlashSale thành công cho ngày {date:dd/MM/yyyy} (tất cả slot)";
                }

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

                var pageIndex = Math.Max(0, (filter.PageIndex ?? 1) - 1); 
                var pageSize = Math.Max(1, Math.Min(100, filter.PageSize ?? 10)); 
                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                if (pageIndex >= totalPages && totalCount > 0)
                {
                    pageIndex = Math.Max(0, totalPages - 1);
                }

                var flashSaleList = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();

                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
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

                        ProductImage? primaryImage = null;

                        if (f.VariantId.HasValue)
                        {
                            primaryImage = await _productImageRepository.GetPrimaryImageAsync(f.ProductId, f.VariantId);

                            if (primaryImage == null)
                            {
                                primaryImage = await _productImageRepository.GetPrimaryImageAsync(f.ProductId, null);
                            }
                        }
                        else
                        {
                            primaryImage = await _productImageRepository.GetPrimaryImageAsync(f.ProductId, null);
                        }

                        productImageUrl = primaryImage?.ImageUrl;

                        if (f.VariantId.HasValue)
                        {
                            variantName = await GetVariantNameAsync(f.VariantId.Value) ?? "";
                        }
                    }
                    catch (Exception)
                    {
                    }
                    var (price, stock) = await GetPriceAndStockAsync(f.ProductId, f.VariantId);

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
                        ProductImageUrl = productImageUrl,
                        Price = price,
                        Stock = stock

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
        private async Task<(decimal Price, int Stock)> GetPriceAndStockAsync(Guid productId, Guid? variantId)
        {
            try
            {
                if (variantId.HasValue && variantId.Value != Guid.Empty)
                {
                    var variant = await _productVariantRepository.GetByIdAsync(variantId.Value.ToString());
                    if (variant != null)
                    {
                        return (variant.Price, variant.Stock);
                    }
                }

                var product = await _productRepository.GetByIdAsync(productId.ToString());
                if (product != null)
                {
                    return (product.BasePrice, product.StockQuantity);
                }
            }
            catch (Exception)
            {
            }

            return (0m, 0);
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
                var remainingQuantity = existingFlashSale.QuantityAvailable - existingFlashSale.QuantitySold;
                if (remainingQuantity > 0)
                {
                    if (existingFlashSale.VariantId.HasValue)
                    {
                        // ✅ NEW: Release reserved stock for Variant
                        var variant = await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString());
                        if (variant != null)
                        {
                            variant.ReleaseReservedStock(remainingQuantity, userId);
                            await _productVariantRepository.ReplaceAsync(variant.Id.ToString(), variant);
                        }
                    }
                    else
                    {
                        // ✅ EXISTING: Release reserved stock for Product
                        existingProduct.RemoveReserveStock(remainingQuantity);
                        await _productRepository.ReplaceAsync(existingProduct.Id.ToString(), existingProduct);
                    }
                }
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

            // ✅ FIX: Handle quantity changes for BOTH Product AND Variant
            if (request.QuantityAvailable.HasValue)
            {
                var oldQuantity = existingFlashSale.QuantityAvailable;
                var newQuantity = request.QuantityAvailable.Value;
                var quantityDifference = newQuantity - oldQuantity;

                if (quantityDifference != 0)
                {
                    if (existingFlashSale.VariantId.HasValue)
                    {
                        // ✅ NEW: Handle Variant quantity changes
                        var variant = await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString());
                        if (variant == null)
                        {
                            response.Success = false;
                            response.Message = "Không tìm thấy variant";
                            return response;
                        }

                        var currentAvailableStock = variant.GetTotalStock(); // Stock + ReserveStock

                        if (newQuantity > currentAvailableStock)
                        {
                            response.Success = false;
                            response.Message = $"Variant không đủ tồn kho để áp dụng FlashSale (yêu cầu: {newQuantity}, khả dụng: {currentAvailableStock})";
                            return response;
                        }

                        if (quantityDifference > 0)
                        {
                            // Tăng số lượng: Reserve thêm stock từ variant
                            if (!variant.CanReserveStock(quantityDifference))
                            {
                                response.Success = false;
                                response.Message = $"Variant không đủ stock để tăng số lượng FlashSale lên {newQuantity}";
                                return response;
                            }
                            variant.ReserveStockForFlashSale(quantityDifference, userId);
                            await _productVariantRepository.ReplaceAsync(variant.Id.ToString(), variant);
                        }
                        else if (quantityDifference < 0)
                        {
                            // Giảm số lượng: Release reserved stock về variant
                            var returnAmount = Math.Abs(quantityDifference);
                            variant.ReleaseReservedStock(returnAmount, userId);
                            await _productVariantRepository.ReplaceAsync(variant.Id.ToString(), variant);
                        }
                    }
                    else
                    {
                        // ✅ EXISTING: Handle Product quantity changes
                        var currentAvailableStock = existingProduct.StockQuantity + existingProduct.ReserveStock;

                        if (newQuantity > currentAvailableStock)
                        {
                            response.Success = false;
                            response.Message = $"Sản phẩm không đủ tồn kho để áp dụng FlashSale (yêu cầu: {newQuantity}, khả dụng: {currentAvailableStock})";
                            return response;
                        }

                        if (quantityDifference > 0)
                        {
                            // Tăng số lượng: Reserve thêm stock
                            bool reserveSuccess = existingProduct.AddReserveStock(quantityDifference);
                            if (!reserveSuccess)
                            {
                                response.Success = false;
                                response.Message = $"Không đủ stock để tăng số lượng FlashSale lên {newQuantity}";
                                return response;
                            }
                            await _productRepository.ReplaceAsync(existingProduct.Id.ToString(), existingProduct);
                        }
                        else if (quantityDifference < 0)
                        {
                            var returnAmount = Math.Abs(quantityDifference);
                            existingProduct.RemoveReserveStock(returnAmount);
                            await _productRepository.ReplaceAsync(existingProduct.Id.ToString(), existingProduct);
                        }
                    }
                }
            }

            // ✅ Validate giá và số lượng cho cả Product và Variant
            if (request.QuantityAvailable.HasValue)
            {
                var maxStock = existingFlashSale.VariantId.HasValue && existingFlashSale.VariantId != Guid.Empty
                    ? (await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString()))?.GetTotalStock() ?? 0
                    : existingProduct.StockQuantity + existingProduct.ReserveStock;

                if (request.QuantityAvailable.Value > maxStock)
                {
                    response.Success = false;
                    response.Message = "Không đủ số lượng sản phẩm tồn kho để áp dụng FlashSale";
                    return response;
                }
            }

            var maxPrice = existingFlashSale.VariantId.HasValue && existingFlashSale.VariantId != Guid.Empty
                ? (await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString()))?.Price ?? 0
                : existingProduct.BasePrice;

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
        public async Task<ApiResponse<ShopFlashSaleOverviewDTO>> GetShopFlashSaleOverviewAsync(string shopId, DateTime date)
        {
            var response = new ApiResponse<ShopFlashSaleOverviewDTO>
            {
                Success = true,
                Message = "Lấy thông tin tổng quan FlashSale thành công"
            };

            try
            {
                if (!Guid.TryParse(shopId, out Guid shopGuid))
                {
                    response.Success = false;
                    response.Message = "Shop ID không hợp lệ";
                    return response;
                }

                var overview = new ShopFlashSaleOverviewDTO
                {
                    Date = date.Date,
                    ShopId = shopGuid,
                    ShopName = "Shop",
                    Slots = new List<FlashSaleSlotInfoDTO>()
                };

                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                // Lặp qua tất cả 8 slots
                for (int slot = 1; slot <= 8; slot++)
                {
                    var slotTime = FlashSaleSlotHelper.GetSlotTimeForDate(slot, date.Date);
                    var timeRange = FlashSaleSlotHelper.SlotTimeRanges[slot];

                    // Lấy FlashSales trong slot này
                    var slotFlashSales = await _flashSaleRepository.GetFlashSalesBySlotAndDateAsync(shopGuid, date.Date, slot);

                    var slotInfo = new FlashSaleSlotInfoDTO
                    {
                        Date = date.Date,
                        Slot = slot,
                        SlotTimeRange = $"{timeRange.Start:hh\\:mm} - {timeRange.End:hh\\:mm}",
                        SlotStatus = GetSlotStatus(slotTime.Start, slotTime.End, slotFlashSales.Any()),
                        TotalProducts = slotFlashSales.Count,
                        TotalQuantityAvailable = slotFlashSales.Sum(f => f.QuantityAvailable),
                        TotalQuantitySold = slotFlashSales.Sum(f => f.QuantitySold),
                        TotalRevenue = slotFlashSales.Sum(f => f.FlashSalePrice * f.QuantitySold),
                        Products = new List<DetailFlashSaleDTO>()
                    };

                    // Tạo DetailFlashSaleDTO cho từng sản phẩm
                    foreach (var flashSale in slotFlashSales)
                    {
                        string? productImageUrl = null;
                        string productName = "Unknown Product";
                        string variantName = "";

                        try
                        {
                            var product = await _productRepository.GetByIdAsync(flashSale.ProductId.ToString());
                            productName = product?.ProductName ?? productName;

                            var primaryImage = await _productImageRepository.GetPrimaryImageAsync(flashSale.ProductId, flashSale.VariantId);
                            productImageUrl = primaryImage?.ImageUrl;

                            if (flashSale.VariantId.HasValue)
                            {
                                variantName = await GetVariantNameAsync(flashSale.VariantId.Value) ?? "";
                            }
                        }
                        catch (Exception)
                        {
                            // Silent fail
                        }

                        slotInfo.Products.Add(new DetailFlashSaleDTO
                        {
                            Id = flashSale.Id,
                            ProductId = flashSale.ProductId,
                            VariantId = flashSale.VariantId,
                            FlashSalePrice = flashSale.FlashSalePrice,
                            QuantityAvailable = flashSale.QuantityAvailable,
                            QuantitySold = flashSale.QuantitySold,
                            Slot = flashSale.Slot,
                            IsActive = !flashSale.IsDeleted && flashSale.StartTime <= DateTime.UtcNow && flashSale.EndTime >= DateTime.UtcNow,
                            StartTime = TimeZoneInfo.ConvertTimeFromUtc(flashSale.StartTime, tz),
                            EndTime = TimeZoneInfo.ConvertTimeFromUtc(flashSale.EndTime, tz),
                            ProductName = productName,
                            VariantName = variantName,
                            ProductImageUrl = productImageUrl
                        });
                    }

                    overview.Slots.Add(slotInfo);
                }

                // Tính tổng cho overview
                overview.TotalActiveSlots = overview.Slots.Count(s => s.TotalProducts > 0);
                overview.TotalProducts = overview.Slots.Sum(s => s.TotalProducts);
                overview.TotalQuantityAvailable = overview.Slots.Sum(s => s.TotalQuantityAvailable);
                overview.TotalQuantitySold = overview.Slots.Sum(s => s.TotalQuantitySold);
                overview.TotalRevenue = overview.Slots.Sum(s => s.TotalRevenue);

                response.Data = overview;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lấy thông tin tổng quan FlashSale: " + ex.Message;
                return response;
            }
        }
        public async Task<ApiResponse<List<FlashSaleSlotSimpleDTO>>> GetShopFlashSaleSimpleAsync(string shopId)
        {
            var response = new ApiResponse<List<FlashSaleSlotSimpleDTO>>
            {
                Success = true,
                Message = "Lấy thông tin tổng quan FlashSale thành công",
                Data = new List<FlashSaleSlotSimpleDTO>()
            };

            try
            {
                if (!Guid.TryParse(shopId, out Guid shopGuid))
                {
                    response.Success = false;
                    response.Message = "Shop ID không hợp lệ";
                    return response;
                }

                var allFlashSales = await _flashSaleRepository.GetByShopIdAsync(shopGuid);
                var activeFlashSales = allFlashSales.Where(f => !f.IsDeleted).ToList();

                // Nhóm theo ngày và slot
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                var groupedByDateAndSlot = activeFlashSales
                    .GroupBy(f => new {
                        Date = TimeZoneInfo.ConvertTimeFromUtc(f.StartTime, tz).Date,
                        Slot = f.Slot
                    })
                    .ToList();
                foreach (var group in groupedByDateAndSlot.OrderBy(g => g.Key.Date).ThenBy(g => g.Key.Slot))
                {
                    var date = group.Key.Date;
                    var slot = group.Key.Slot;
                    var flashSalesInSlot = group.ToList();

                    var utcDate = TimeZoneInfo.ConvertTimeToUtc(date, tz);
                    var slotTime = FlashSaleSlotHelper.GetSlotTimeForDate(slot, utcDate);

                    var slotInfo = new FlashSaleSlotSimpleDTO
                    {
                        Date = date,
                        Slot = slot,
                        Status = GetSlotStatus(slotTime.Start, slotTime.End, flashSalesInSlot.Any()),
                        TotalProduct = flashSalesInSlot.Count
                    };

                    response.Data.Add(slotInfo);
                }

                response.Message = $"Lấy thông tin tổng quan FlashSale thành công. " +
                    $"Tổng: {response.Data.Count} slot, " +
                    $"từ {activeFlashSales.Count} FlashSale";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lấy thông tin tổng quan FlashSale: " + ex.Message;
                return response;
            }
        }
        public async Task<ApiResponse<bool>> DeleteFlashSaleSlotAsync(DeleteFlashSaleSlotDTO request, string userId, string shopId)
        {
            var response = new ApiResponse<bool>()
            {
                Success = true,
                Message = "Xóa FlashSale slot thành công",
                Data = true
            };

            try
            {
                if (!Guid.TryParse(shopId, out Guid shopGuid))
                {
                    response.Success = false;
                    response.Message = "Shop ID không hợp lệ";
                    return response;
                }

                // Validate slot
                if (!FlashSaleSlotHelper.SlotTimeRanges.ContainsKey(request.Slot))
                {
                    response.Success = false;
                    response.Message = $"Slot {request.Slot} không hợp lệ. Slot hợp lệ từ 1-8";
                    return response;
                }

                // Lấy tất cả FlashSale trong slot và ngày của shop
                var slotFlashSales = await _flashSaleRepository.GetFlashSalesBySlotAndDateAsync(shopGuid, request.Date.Date, request.Slot);

                if (!slotFlashSales.Any())
                {
                    response.Success = false;
                    response.Message = $"Không tìm thấy FlashSale nào trong slot {request.Slot} ngày {request.Date:dd/MM/yyyy}";
                    return response;
                }

                // Kiểm tra xem có FlashSale nào đang diễn ra không
                var now = DateTime.UtcNow;
                var activeFlashSales = slotFlashSales.Where(f => f.StartTime <= now && f.EndTime >= now && !f.IsDeleted).ToList();

                if (activeFlashSales.Any())
                {
                    response.Success = false;
                    response.Message = $"Không thể xóa slot vì có {activeFlashSales.Count} FlashSale đang diễn ra";
                    return response;
                }

                // Xóa tất cả FlashSale trong slot
                int deletedCount = 0;
                foreach (var flashSale in slotFlashSales)
                {
                    if (!flashSale.IsDeleted)
                    {
                        flashSale.Delete(userId);
                        await _flashSaleRepository.ReplaceAsync(flashSale.Id.ToString(), flashSale);
                        deletedCount++;
                    }
                }

                response.Message = $"Đã xóa {deletedCount} FlashSale trong slot {request.Slot} ngày {request.Date:dd/MM/yyyy}";

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi xóa FlashSale slot: " + ex.Message;
                return response;
            }
        }
        /// <summary>
        /// ✅ NEW: Cập nhật đơn giản chỉ giá và số lượng FlashSale
        /// </summary>
        //public async Task<ApiResponse<DetailFlashSaleDTO>> UpdateFlashSalePriceQuantityAsync(UpdateFlashSalePriceQuantityDTO request, string flashSaleId, string userId, string shopId)
        //{
        //    var response = new ApiResponse<DetailFlashSaleDTO>()
        //    {
        //        Success = true,
        //        Message = "Cập nhật FlashSale thành công",
        //    };

        //    try
        //    {
        //        var existingFlashSale = await _flashSaleRepository.GetByIdAsync(flashSaleId);
        //        if (existingFlashSale == null || existingFlashSale.IsDeleted == true)
        //        {
        //            response.Success = false;
        //            response.Message = "Không tìm thấy FlashSale";
        //            return response;
        //        }

        //        var existingProduct = await _productRepository.GetByIdAsync(existingFlashSale.ProductId.ToString());
        //        if (existingProduct == null || existingProduct.ShopId.ToString() != shopId)
        //        {
        //            response.Success = false;
        //            response.Message = "Bạn không có quyền cập nhật FlashSale cho sản phẩm này";
        //            return response;
        //        }

        //        // Không cho phép cập nhật nếu đang diễn ra
        //        var now = DateTime.UtcNow;
        //        if (existingFlashSale.StartTime <= now && existingFlashSale.EndTime >= now)
        //        {
        //            response.Success = false;
        //            response.Message = "Không thể cập nhật FlashSale đang diễn ra";
        //            return response;
        //        }

        //        // Validate giá và số lượng
        //        if (request.QuantityAvailable.HasValue)
        //        {
        //            var maxStock = existingFlashSale.VariantId.HasValue && existingFlashSale.VariantId != Guid.Empty
        //                ? (await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString()))?.Stock ?? 0
        //                : existingProduct.StockQuantity;

        //            if (request.QuantityAvailable.Value > maxStock)
        //            {
        //                response.Success = false;
        //                response.Message = "Không đủ số lượng sản phẩm tồn kho để áp dụng FlashSale";
        //                return response;
        //            }
        //        }

        //        if (request.FLashSalePrice.HasValue)
        //        {
        //            var maxPrice = existingFlashSale.VariantId.HasValue && existingFlashSale.VariantId != Guid.Empty
        //                ? (await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString()))?.Price ?? 0
        //                : existingProduct.BasePrice;

        //            if (request.FLashSalePrice.Value >= maxPrice)
        //            {
        //                response.Success = false;
        //                response.Message = "Giá FlashSale phải thấp hơn giá sản phẩm";
        //                return response;
        //            }
        //        }

        //        // Cập nhật các field được yêu cầu
        //        if (request.FLashSalePrice.HasValue)
        //            existingFlashSale.FlashSalePrice = request.FLashSalePrice.Value;

        //        if (request.QuantityAvailable.HasValue)
        //            existingFlashSale.QuantityAvailable = request.QuantityAvailable.Value;

        //        existingFlashSale.SetModifier(userId);

        //        await _flashSaleRepository.ReplaceAsync(existingFlashSale.Id.ToString(), existingFlashSale);

        //        // Trả về kết quả với thông tin đầy đủ
        //        var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        //        string? productImageUrl = null;
        //        string productName = existingProduct.ProductName;
        //        string variantName = "";

        //        try
        //        {
        //            var primaryImage = await _productImageRepository.GetPrimaryImageAsync(existingFlashSale.ProductId, existingFlashSale.VariantId);
        //            productImageUrl = primaryImage?.ImageUrl;

        //            if (existingFlashSale.VariantId.HasValue)
        //            {
        //                variantName = await GetVariantNameAsync(existingFlashSale.VariantId.Value) ?? "";
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            // Silent fail
        //        }

        //        var result = new DetailFlashSaleDTO()
        //        {
        //            Id = existingFlashSale.Id,
        //            ProductId = existingFlashSale.ProductId,
        //            VariantId = existingFlashSale.VariantId,
        //            FlashSalePrice = existingFlashSale.FlashSalePrice,
        //            QuantityAvailable = existingFlashSale.QuantityAvailable,
        //            QuantitySold = existingFlashSale.QuantitySold,
        //            StartTime = TimeZoneInfo.ConvertTimeFromUtc(existingFlashSale.StartTime, tz),
        //            EndTime = TimeZoneInfo.ConvertTimeFromUtc(existingFlashSale.EndTime, tz),
        //            Slot = existingFlashSale.Slot,
        //            IsActive = existingFlashSale.IsValid(),
        //            ProductName = productName,
        //            VariantName = variantName,
        //            ProductImageUrl = productImageUrl
        //        };

        //        response.Data = result;
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Success = false;
        //        response.Message = "Lỗi khi cập nhật FlashSale: " + ex.Message;
        //        return response;
        //    }
        //}

        public async Task<ApiResponse<DetailFlashSaleDTO>> UpdateFlashSalePriceQuantityAsync(UpdateFlashSalePriceQuantityDTO request, string flashSaleId, string userId, string shopId)
        {
            var response = new ApiResponse<DetailFlashSaleDTO>()
            {
                Success = true,
                Message = "Cập nhật FlashSale thành công",
            };

            try
            {
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

                var now = DateTime.UtcNow;
                if (existingFlashSale.StartTime <= now && existingFlashSale.EndTime >= now)
                {
                    response.Success = false;
                    response.Message = "Không thể cập nhật FlashSale đang diễn ra";
                    return response;
                }

                // ✅ FIX: Handle quantity changes for both Product and Variant
                if (request.QuantityAvailable.HasValue)
                {
                    var oldQuantity = existingFlashSale.QuantityAvailable;
                    var newQuantity = request.QuantityAvailable.Value;
                    var quantityDifference = newQuantity - oldQuantity;

                    if (quantityDifference != 0)
                    {
                        if (existingFlashSale.VariantId.HasValue)
                        {
                            // ✅ NEW: Handle Variant quantity changes
                            var variant = await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString());
                            if (variant == null)
                            {
                                response.Success = false;
                                response.Message = "Không tìm thấy variant";
                                return response;
                            }

                            var currentAvailableStock = variant.GetTotalStock(); 

                            if (newQuantity > currentAvailableStock)
                            {
                                response.Success = false;
                                response.Message = $"Variant không đủ tồn kho để áp dụng FlashSale (yêu cầu: {newQuantity}, khả dụng: {currentAvailableStock})";
                                return response;
                            }

                            if (quantityDifference > 0)
                            {
                                // Tăng số lượng: Reserve thêm stock từ variant
                                if (!variant.CanReserveStock(quantityDifference))
                                {
                                    response.Success = false;
                                    response.Message = $"Variant không đủ stock để tăng số lượng FlashSale lên {newQuantity}";
                                    return response;
                                }
                                variant.ReserveStockForFlashSale(quantityDifference, userId);
                                await _productVariantRepository.ReplaceAsync(variant.Id.ToString(), variant);

                              
                            }
                            else if (quantityDifference < 0)
                            {
                                // Giảm số lượng: Release reserved stock về variant
                                var returnAmount = Math.Abs(quantityDifference);
                                variant.ReleaseReservedStock(returnAmount, userId);
                                await _productVariantRepository.ReplaceAsync(variant.Id.ToString(), variant);
                            }
                        }
                        else
                        {
                            // ✅ EXISTING: Handle Product quantity changes
                            var currentAvailableStock = existingProduct.StockQuantity + existingProduct.ReserveStock;

                            if (newQuantity > currentAvailableStock)
                            {
                                response.Success = false;
                                response.Message = $"Sản phẩm không đủ tồn kho để áp dụng FlashSale (yêu cầu: {newQuantity}, khả dụng: {currentAvailableStock})";
                                return response;
                            }

                            if (quantityDifference > 0)
                            {
                                // Tăng số lượng: Reserve thêm stock
                                bool reserveSuccess = existingProduct.AddReserveStock(quantityDifference);
                                if (!reserveSuccess)
                                {
                                    response.Success = false;
                                    response.Message = $"Không đủ stock để tăng số lượng FlashSale lên {newQuantity}";
                                    return response;
                                }
                                await _productRepository.ReplaceAsync(existingProduct.Id.ToString(), existingProduct);
                            }
                            else if (quantityDifference < 0)
                            {
                                // Giảm số lượng: Release reserved stock
                                var returnAmount = Math.Abs(quantityDifference);
                                existingProduct.RemoveReserveStock(returnAmount);
                                await _productRepository.ReplaceAsync(existingProduct.Id.ToString(), existingProduct);
                            }
                        }
                    }
                }

                // ✅ Validate giá nếu có thay đổi
                if (request.FLashSalePrice.HasValue)
                {
                    var maxPrice = existingFlashSale.VariantId.HasValue && existingFlashSale.VariantId != Guid.Empty
                        ? (await _productVariantRepository.GetByIdAsync(existingFlashSale.VariantId.ToString()))?.Price ?? 0
                        : existingProduct.BasePrice;

                    if (request.FLashSalePrice.Value >= maxPrice)
                    {
                        response.Success = false;
                        response.Message = "Giá FlashSale phải thấp hơn giá sản phẩm";
                        return response;
                    }

                    existingFlashSale.FlashSalePrice = request.FLashSalePrice.Value;
                }

                if (request.QuantityAvailable.HasValue)
                {
                    existingFlashSale.QuantityAvailable = request.QuantityAvailable.Value;
                }

                existingFlashSale.SetModifier(userId);
                await _flashSaleRepository.ReplaceAsync(existingFlashSale.Id.ToString(), existingFlashSale);

                // ✅ Trả về kết quả với thông tin đầy đủ
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                string? productImageUrl = null;
                string productName = existingProduct.ProductName;
                string variantName = "";

                try
                {
                    var primaryImage = await _productImageRepository.GetPrimaryImageAsync(existingFlashSale.ProductId, existingFlashSale.VariantId);
                    productImageUrl = primaryImage?.ImageUrl;

                    if (existingFlashSale.VariantId.HasValue)
                    {
                        variantName = await GetVariantNameAsync(existingFlashSale.VariantId.Value) ?? "";
                    }
                }
                catch (Exception)
                {
                    // Silent fail
                }

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
                    ProductName = productName,
                    VariantName = variantName,
                    ProductImageUrl = productImageUrl
                };

                response.Data = result;

                return response;
            }
            catch (Exception ex)
            {
                
                response.Success = false;
                response.Message = "Lỗi khi cập nhật FlashSale: " + ex.Message;
                return response;
            }
        }

        /// <summary>
        /// Helper method để xác định trạng thái slot
        /// </summary>
        private string GetSlotStatus(DateTime startTime, DateTime endTime, bool hasProducts)
        {
            if (!hasProducts) return "Empty";

            var now = DateTime.UtcNow;
            if (now < startTime) return "Upcoming";
            if (now >= startTime && now <= endTime) return "Active";
            if (now > endTime) return "Expired";

            return "Unknown";
        }

        public async Task<ApiResponse<DetailFlashSaleDTO>> UpdateFlashSaleStock(string flashSaleId, int quantity)
        {
            var response = new ApiResponse<DetailFlashSaleDTO>
            {
                Success = true,
                Message = "Cập nhật QuantitySold cho FlashSale thành công",
            };

            try
            {
                if (string.IsNullOrWhiteSpace(flashSaleId))
                {
                    response.Success = false;
                    response.Message = "flashSaleId không hợp lệ";
                    return response;
                }

                if (quantity <= 0)
                {
                    response.Success = false;
                    response.Message = "Số lượng mua phải lớn hơn 0";
                    return response;
                }

                var flashSale = await _flashSaleRepository.GetByIdAsync(flashSaleId);
                if (flashSale == null || flashSale.IsDeleted)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy FlashSale";
                    return response;
                }

                // Không chạm vào stock sản phẩm/variant – chỉ tăng QuantitySold
                var remaining = flashSale.QuantityAvailable - flashSale.QuantitySold;
                if (quantity > remaining)
                {
                    response.Success = false;
                    response.Message = $"Số lượng còn lại của FlashSale không đủ (yêu cầu {quantity}, còn {remaining})";
                    return response;
                }

                flashSale.QuantitySold += quantity;
                flashSale.SetModifier("system"); // hoặc truyền userId nếu bạn có
                await _flashSaleRepository.ReplaceAsync(flashSale.Id.ToString(), flashSale);

                // Build DTO trả về
                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                string? productImageUrl = null;
                string productName = "Unknown Product";
                string variantName = "";

                try
                {
                    var product = await _productRepository.GetByIdAsync(flashSale.ProductId.ToString());
                    productName = product?.ProductName ?? productName;

                    var primaryImage = await _productImageRepository.GetPrimaryImageAsync(flashSale.ProductId, flashSale.VariantId);
                    productImageUrl = primaryImage?.ImageUrl;

                    if (flashSale.VariantId.HasValue)
                    {
                        variantName = await GetVariantNameAsync(flashSale.VariantId.Value) ?? "";
                    }
                }
                catch { /* silent */ }

                var (price, stock) = await GetPriceAndStockAsync(flashSale.ProductId, flashSale.VariantId);

                response.Data = new DetailFlashSaleDTO
                {
                    Id = flashSale.Id,
                    ProductId = flashSale.ProductId,
                    VariantId = flashSale.VariantId,
                    FlashSalePrice = flashSale.FlashSalePrice,
                    QuantityAvailable = flashSale.QuantityAvailable,
                    QuantitySold = flashSale.QuantitySold,
                    StartTime = TimeZoneInfo.ConvertTimeFromUtc(flashSale.StartTime, tz),
                    EndTime = TimeZoneInfo.ConvertTimeFromUtc(flashSale.EndTime, tz),
                    Slot = flashSale.Slot,
                    IsActive = flashSale.IsValid(),
                    ProductName = productName,
                    VariantName = variantName,
                    ProductImageUrl = productImageUrl,
                    Price = price,
                    Stock = stock
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi cập nhật QuantitySold: " + ex.Message;
                return response;
            }
        }

    }
}