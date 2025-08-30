using LivestreamService.Application.Commands;
using LivestreamService.Application.Commands.LiveStreamService;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LivestreamService.Api.Controllers
{
    [ApiController]
    [Route("api/livestream-products")]
    public class LivestreamProductController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<LivestreamProductController> _logger;

        public LivestreamProductController(
            IMediator mediator,
            ICurrentUserService currentUserService,
            ILogger<LivestreamProductController> logger)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// Thêm sản phẩm vào livestream
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Seller,Moderator")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamProductDTO>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateLivestreamProduct([FromBody] CreateLivestreamProductDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
            }

            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new CreateLivestreamProductCommand
                {
                    LivestreamId = request.LivestreamId,
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    Price = request.Price,
                    Stock = request.Stock,
                    IsPin = request.IsPin,
                    //FlashSaleId = request.FlashSaleId,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Created($"/api/livestream-products/{result.Id}", ApiResponse<LivestreamProductDTO>.SuccessResult(result, "Đã thêm sản phẩm vào livestream thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi thêm sản phẩm: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm trong livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LivestreamProductDTO>>), 200)]
        public async Task<IActionResult> GetLivestreamProducts(Guid livestreamId)
        {
            try
            {
                var query = new GetLivestreamProductsQuery { LivestreamId = livestreamId };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<IEnumerable<LivestreamProductDTO>>.SuccessResult(result, "Lấy danh sách sản phẩm thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm đã ghim trong livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}/pinned")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LivestreamProductDTO>>), 200)]
        public async Task<IActionResult> GetPinnedProducts(Guid livestreamId, [FromQuery] int limit = 5)
        {
            try
            {
                var query = new GetPinnedProductsQuery
                {
                    LivestreamId = livestreamId,
                    Limit = limit
                };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<IEnumerable<LivestreamProductDTO>>.SuccessResult(result, "Lấy danh sách sản phẩm đã ghim thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm đã ghim");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm bán chạy trong livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}/best-selling")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LivestreamProductSummaryDTO>>), 200)]
        public async Task<IActionResult> GetBestSellingProducts(Guid livestreamId, [FromQuery] int limit = 10)
        {
            try
            {
                var query = new GetBestSellingProductsQuery
                {
                    LivestreamId = livestreamId,
                    Limit = limit
                };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<IEnumerable<LivestreamProductSummaryDTO>>.SuccessResult(result, "Lấy danh sách sản phẩm bán chạy thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách sản phẩm bán chạy");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy chi tiết sản phẩm trong livestream
        /// </summary>
        [HttpGet("{id}/detail")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<LivestreamProductDetailDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetLivestreamProductDetail(Guid id)
        {
            try
            {
                var query = new GetLivestreamProductDetailQuery { Id = id };
                var result = await _mediator.Send(query);

                return Ok(ApiResponse<LivestreamProductDetailDTO>.SuccessResult(result, "Lấy chi tiết sản phẩm thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết sản phẩm livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật sản phẩm trong livestream
        /// </summary>
        /// </summary>
        [HttpPut("livestream/{livestreamId}/product/{productId}/variant/{variantId}")]
        //[Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamProductDTO>), 200)]
        public async Task<IActionResult> UpdateLivestreamProduct(
            Guid livestreamId,
            string productId,
            string? variantId,
            [FromBody] UpdateLivestreamProductDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new UpdateLivestreamProductCommand
                {
                    LivestreamId = livestreamId,
                    ProductId = productId,
                    VariantId = variantId ?? string.Empty,
                    Price = request.Price,
                    Stock = request.Stock,
                    IsPin = request.IsPin,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamProductDTO>.SuccessResult(result, "Cập nhật sản phẩm thành công"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Ghim/bỏ ghim sản phẩm
        /// </summary>
        [HttpPatch("livestream/{livestreamId}/product/{productId}/variant/{variantId}/pin")]
       // [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamProductDTO>), 200)]
        public async Task<IActionResult> PinProduct(
            Guid livestreamId,
            string productId,
            string? variantId,
            [FromBody] PinProductDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new PinProductCommand
                {
                    LivestreamId = livestreamId,
                    ProductId = productId,
                    VariantId = variantId ?? string.Empty,
                    IsPin = request.IsPin,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamProductDTO>.SuccessResult(result, request.IsPin ? "Đã ghim sản phẩm" : "Đã bỏ ghim sản phẩm"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghim/bỏ ghim sản phẩm");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật số lượng tồn kho sản phẩm
        /// </summary>
        [HttpPatch("livestream/{livestreamId}/product/{productId}/variant/{variantId}/stock")]
       // [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamProductDTO>), 200)]
        public async Task<IActionResult> UpdateStock(
            Guid livestreamId,
            string productId,
            string? variantId,
            [FromBody] UpdateStockDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new UpdateStockCommand
                {
                    LivestreamId = livestreamId,
                    ProductId = productId,
                    VariantId = variantId ?? string.Empty,
                    Stock = request.Stock,
                    Price = request.Price,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamProductDTO>.SuccessResult(result, "Đã cập nhật số lượng"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật số lượng tồn kho");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }


        /// <summary>
        /// Xóa sản phẩm khỏi livestream
        /// </summary>
        [HttpDelete("livestream/{livestreamId}/product/{productId}/variant/{variantId}")]
       // [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> DeleteLivestreamProduct(
            Guid livestreamId,
            string productId,
            string variantId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new DeleteLivestreamProductCommand
                {
                    LivestreamId = livestreamId,
                    ProductId = productId,
                    VariantId = variantId,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Đã xóa sản phẩm khỏi livestream"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
        /// <summary>
        /// Lấy thông tin 1 sản phẩm với tất cả phân loại trong livestream
        /// </summary>
        [HttpGet("livestream/{livestreamId}/product/{productId}")]
       // [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ProductLiveStreamDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetProductLiveStream(Guid livestreamId, string productId)
        {
            try
            {
                var query = new GetProductLiveStreamQuery(livestreamId, productId);
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Sản phẩm {productId} không tồn tại trong livestream {livestreamId}"));
                }

                return Ok(ApiResponse<ProductLiveStreamDTO>.SuccessResult(result, "Lấy thông tin sản phẩm với các phân loại thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin sản phẩm {ProductId} trong livestream {LivestreamId}", productId, livestreamId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
        /// <summary>
        /// Lấy sản phẩm trong livestream theo SKU
        /// </summary>
        [HttpGet("livestream/{livestreamId}/sku/{sku}")]
        //[Authorize]
        [ProducesResponseType(typeof(ApiResponse<LivestreamProductDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetLivestreamProductBySku(Guid livestreamId, string sku)
        {
            try
            {
                _logger.LogInformation("Getting product with SKU {Sku} for livestream {LivestreamId}", sku, livestreamId);

                var query = new GetLivestreamProductBySkuQuery
                {
                    LivestreamId = livestreamId,
                    Sku = sku
                };

                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResult($"Không tìm thấy sản phẩm có SKU '{sku}' trong livestream này"));
                }

                return Ok(ApiResponse<LivestreamProductDTO>.SuccessResult(result, "Lấy thông tin sản phẩm thành công"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm theo SKU {Sku} trong livestream {LivestreamId}", sku, livestreamId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Lấy nhiều sản phẩm trong livestream theo danh sách SKU
        /// </summary>
        [HttpPost("livestream/{livestreamId}/skus")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LivestreamProductDTO>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetLivestreamProductsBySkus(Guid livestreamId, [FromBody] GetProductsBySkusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Dữ liệu không hợp lệ"));
                }

                if (request.Skus == null || !request.Skus.Any())
                {
                    return BadRequest(ApiResponse<object>.ErrorResult("Danh sách SKU không được rỗng"));
                }

                _logger.LogInformation("Getting products with SKUs [{Skus}] for livestream {LivestreamId}",
                    string.Join(", ", request.Skus), livestreamId);

                var query = new GetLivestreamProductsBySkusQuery
                {
                    LivestreamId = livestreamId,
                    Skus = request.Skus
                };

                var results = await _mediator.Send(query);

                return Ok(ApiResponse<IEnumerable<LivestreamProductDTO>>.SuccessResult(results,
                    $"Tìm thấy {results.Count()} sản phẩm trong tổng số {request.Skus.Count()} SKU"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm theo danh sách SKU trong livestream {LivestreamId}", livestreamId);
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
        /// <summary>
        /// Xóa sản phẩm khỏi livestream bằng ID
        /// </summary>
        [HttpDelete("{id}")]
       // [Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteLivestreamProductById(Guid id)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new DeleteLivestreamProductByIdCommand
                {
                    Id = id,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<bool>.SuccessResult(result, "Đã xóa sản phẩm khỏi livestream"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi livestream");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Ghim/bỏ ghim sản phẩm bằng ID
        /// </summary>
        [HttpPatch("{id}/pin")]
        //[Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamProductDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> PinProductById(
            Guid id,
            [FromBody] PinProductDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new PinProductByIdCommand
                {
                    Id = id,
                    IsPin = request.IsPin,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamProductDTO>.SuccessResult(result, request.IsPin ? "Đã ghim sản phẩm" : "Đã bỏ ghim sản phẩm"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghim/bỏ ghim sản phẩm");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cập nhật số lượng tồn kho sản phẩm bằng ID
        /// </summary>
        [HttpPatch("{id}/stock")]
        //[Authorize(Roles = "Seller")]
        [ProducesResponseType(typeof(ApiResponse<LivestreamProductDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateStockById(
            Guid id,
            [FromBody] UpdateStockDTO request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var command = new UpdateStockByIdCommand
                {
                    Id = id,
                    Stock = request.Stock,
                    SellerId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(ApiResponse<LivestreamProductDTO>.SuccessResult(result, "Đã cập nhật số lượng"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật số lượng tồn kho");
                return BadRequest(ApiResponse<object>.ErrorResult($"Lỗi: {ex.Message}"));
            }
        }
    }
}