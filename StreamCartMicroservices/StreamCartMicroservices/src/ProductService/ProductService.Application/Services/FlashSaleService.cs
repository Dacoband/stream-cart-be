using Appwrite.Models;
using ProductService.Application.DTOs.FlashSale;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class FlashSaleService : IFlashSaleService
    {
        private readonly IFlashSaleRepository _flashSaleRepository; 
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;

        public FlashSaleService(IFlashSaleRepository flashSaleRepository, IProductRepository productRepository, IProductVariantRepository productVariantRepository)
        {
            _flashSaleRepository = flashSaleRepository;
            _productRepository = productRepository; 
            _productVariantRepository = productVariantRepository;
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

            var existingProduct = await _productRepository.GetByIdAsync(request.ProductId.ToString());
            if (existingProduct == null || existingProduct.IsActive == false || existingProduct.IsDeleted == true)
            {
                response.Success = false;
                response.Message = "Không tìm thấy sản phẩm cần áp dụng FlashSale";
                return response;
            }

            if (existingProduct.ShopId.ToString() != shopId)
            {
                response.Success = false;
                response.Message = "Bạn không có quyền tạo FlashSale cho sản phẩm này";
                return response;
            }

            if (request.VariantId == null || request.VariantId.Count == 0)
            {
                if (request.QuantityAvailable > existingProduct.StockQuantity)
                {
                    response.Success = false;
                    response.Message = "Không đủ số lượng sản phẩm tồn kho để áp dụng FlashSale";
                    return response;
                }

                if (request.FLashSalePrice > existingProduct.BasePrice)
                {
                    response.Success = false;
                    response.Message = "Giá FlashSale phải thấp hơn giá sản phẩm";
                    return response;
                }

                var existingFlashSale  = await _flashSaleRepository.GetByTimeAndProduct(request.StartTime, request.EndTime, request.ProductId, null);
                if (existingFlashSale != null && existingFlashSale.Count > 0)
                {
                    response.Success = false;
                    response.Message = "Chỉ có thể áp dụng 1 FlashSale cho cùng 1 thời điểm";
                    return response;
                }
                request.ConvertToUtc();
                var flashSaleCreated = new FlashSale()
                {
                    ProductId = request.ProductId,
                    VariantId = null,
                    FlashSalePrice = request.FLashSalePrice,
                    QuantityAvailable = request.QuantityAvailable ?? existingProduct.StockQuantity,
                    QuantitySold = 0,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                };
                flashSaleCreated.SetCreator(userId);
                
                await _flashSaleRepository.InsertAsync(flashSaleCreated);
                var result = new DetailFlashSaleDTO()
                {
                    Id = flashSaleCreated.Id,
                    ProductId = request.ProductId,
                    VariantId = Guid.Empty,
                    FlashSalePrice = flashSaleCreated.FlashSalePrice,
                    QuantityAvailable = request?.QuantityAvailable ?? existingProduct.StockQuantity,
                    QuantitySold = 0,
                    StartTime = flashSaleCreated.StartTime,
                    EndTime = flashSaleCreated.EndTime,
                    IsActive = true,
                };
                response.Data.Add(result);
            }
            else
            {
                foreach (var variantId in request.VariantId)
                {
                    var existingVariant = await _productVariantRepository.GetByIdAsync(variantId.ToString());
                    if (existingVariant == null || existingVariant.ProductId != request.ProductId || existingVariant.IsDeleted == true)
                    {
                        errorMessages.Add($"Phân loại {variantId} không hợp lệ hoặc không thuộc sản phẩm.");
                        continue;
                    }

                    if (request.QuantityAvailable > existingVariant.Stock)
                    {
                        errorMessages.Add($"Phân loại {variantId} không đủ tồn kho để áp dụng FlashSale.");
                        continue;
                    }

                    if (request.FLashSalePrice > existingVariant.Price)
                    {
                        errorMessages.Add($"Giá FlashSale cao hơn giá phân loại {variantId}.");
                        continue;
                    }

                    var existingFlashSale = await _flashSaleRepository.GetByTimeAndProduct(request.StartTime, request.EndTime, request.ProductId, existingVariant.Id);
                    if (existingFlashSale.Count > 0)
                    {
                        errorMessages.Add($"Phân loại {variantId} đã có FlashSale trong cùng thời điểm.");
                        continue;
                    }

                    var flashSaleCreated = new FlashSale()
                    {
                        ProductId = request.ProductId,
                        VariantId = existingVariant.Id,
                        FlashSalePrice = request.FLashSalePrice,
                        QuantityAvailable = request.QuantityAvailable ?? existingVariant.Stock,
                        QuantitySold = 0,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                    };
                    flashSaleCreated.SetCreator(userId);
                    await _flashSaleRepository.InsertAsync(flashSaleCreated);
                    var result = new DetailFlashSaleDTO()
                    {
                        Id = flashSaleCreated.Id,
                        ProductId = request.ProductId,
                        VariantId = flashSaleCreated.VariantId,
                        FlashSalePrice = flashSaleCreated.FlashSalePrice,
                        QuantityAvailable = request?.QuantityAvailable ?? existingProduct.StockQuantity,
                        QuantitySold = 0,
                        StartTime = flashSaleCreated.StartTime,
                        EndTime = flashSaleCreated.EndTime,
                        IsActive = true,
                    };
                    response.Data.Add(result);                }

                if (!response.Data.Any())
                {
                    response.Success = false;
                    response.Message = "Không có phân loại nào hợp lệ để áp dụng FlashSale:\n" + string.Join("\n", errorMessages);
                    return response;
                }

                if (errorMessages.Any())
                {
                    response.Message += $" Tuy nhiên, một số phân loại không áp dụng được:\n{string.Join("\n", errorMessages)}";
                }
            }

            return response;
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
            catch (Exception ex) {
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

                // ✅ FIX: Lọc theo IsActive với logic chính xác
                if (filter.IsActive.HasValue)
                {
                    if (filter.IsActive.Value)
                    {
                        // Lọc FlashSale đang hoạt động: không bị xóa, trong thời gian hiệu lực (UTC)
                        query = query.Where(f => !f.IsDeleted &&
                                                f.StartTime <= nowUtc &&
                                                f.EndTime >= nowUtc);
                    }
                    else
                    {
                        // Lọc FlashSale không hoạt động: bị xóa hoặc ngoài thời gian hiệu lực (UTC)
                        query = query.Where(f => f.IsDeleted ||
                                                f.StartTime > nowUtc ||
                                                f.EndTime < nowUtc);
                    }
                }

                // Lọc theo ProductId
                if (filter.ProductId != null && filter.ProductId.Any())
                {
                    query = query.Where(f => filter.ProductId.Contains(f.ProductId));
                }

                // Lọc theo VariantId
                if (filter.VariantId != null && filter.VariantId.Any())
                {
                    query = query.Where(f => f.VariantId.HasValue && filter.VariantId.Contains(f.VariantId.Value));
                }

                // ✅ FIX: Lọc thời gian với UTC
                if (filter.StartDate.HasValue)
                {
                    // Convert local time to UTC for comparison
                    var startUtc = filter.StartDate.Value.Kind == DateTimeKind.Utc
                        ? filter.StartDate.Value
                        : TimeZoneInfo.ConvertTimeToUtc(filter.StartDate.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    query = query.Where(f => f.EndTime >= startUtc);
                }

                if (filter.EndDate.HasValue)
                {
                    // Convert local time to UTC for comparison
                    var endUtc = filter.EndDate.Value.Kind == DateTimeKind.Utc
                        ? filter.EndDate.Value
                        : TimeZoneInfo.ConvertTimeToUtc(filter.EndDate.Value, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    query = query.Where(f => f.StartTime <= endUtc);
                }

                // Sắp xếp
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

                // Phân trang
                var pageIndex = filter.PageIndex ?? 0;
                var pageSize = filter.PageSize ?? 10;
                query = query.Skip(pageIndex * pageSize).Take(pageSize);

                var tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                // ✅ Map DTO với timezone conversion nhất quán
                response.Data = query.Select(f => new DetailFlashSaleDTO
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
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi lọc FlashSale: " + ex.Message;
                return response;
            }
        }

        public async Task<ApiResponse<DetailFlashSaleDTO>> GetFlashSaleById(string id)
        {
            var response = new ApiResponse<DetailFlashSaleDTO>
            {
                Success = true,
                Message = "Lọc danh sách FlashSale thành công",
               
            };
            var flashSale = await _flashSaleRepository.GetByIdAsync(id);
            if (flashSale == null) {
                response.Success = false;
                response.Message = "Không tìm thấy FlashSale";
                return response;
            
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
                IsActive = flashSale.IsValid(),
                QuantitySold = flashSale.QuantitySold,
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
                response.Message = "Bạn không có quyền tạo FlashSale cho sản phẩm này";
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

            if (request.FLashSalePrice.HasValue && request.FLashSalePrice.Value > maxPrice)
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
                await _flashSaleRepository.ReplaceAsync(existingFlashSale.Id.ToString(),existingFlashSale);
                var result = new DetailFlashSaleDTO()
                {
                    Id = existingFlashSale.Id,
                    ProductId = existingFlashSale.ProductId,
                    VariantId = existingFlashSale.VariantId ,
                    FlashSalePrice = existingFlashSale.FlashSalePrice,
                    QuantityAvailable = request?.QuantityAvailable ?? existingProduct.StockQuantity,
                    QuantitySold = 0,
                    StartTime = existingFlashSale.StartTime,
                    EndTime = existingFlashSale.EndTime,
                    IsActive = true,
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

    }
}
