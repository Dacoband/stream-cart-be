using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces
{
    public interface ILivestreamServiceClient
    {
        /// <summary>
        /// Lấy thông tin livestream theo ID
        /// </summary>
        Task<LivestreamInfoDTO?> GetLivestreamByIdAsync(Guid livestreamId);

        /// <summary>
        /// Lấy danh sách livestream theo shop ID
        /// </summary>
        Task<List<LivestreamInfoDTO>> GetLivestreamsByShopIdAsync(Guid shopId);

        /// <summary>
        /// Kiểm tra livestream có tồn tại không
        /// </summary>
        Task<bool> DoesLivestreamExistAsync(Guid livestreamId);

        /// <summary>
        /// Lấy thông tin cơ bản của livestream để hiển thị trong review
        /// </summary>
        Task<LivestreamBasicInfoDTO?> GetLivestreamBasicInfoAsync(Guid livestreamId);
        Task<bool> UpdateProductStockAsync(Guid livestreamId, string productId, string? variantId, int quantityChange, string modifiedBy);
        Task<LivestreamProductPricing?> GetLivestreamProductPricingAsync(Guid livestreamId, string productId, string? variantId);

    }
    public class LivestreamProductPricing
    {
        /// <summary>
        /// ID sản phẩm
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// ID variant (nếu có)
        /// </summary>
        public string? VariantId { get; set; }

        /// <summary>
        /// Giá trong livestream (đã có discount)
        /// </summary>
        public decimal LivestreamPrice { get; set; }

        /// <summary>
        /// Giá gốc của sản phẩm
        /// </summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Số lượng tồn kho trong livestream
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Phần trăm giảm giá
        /// </summary>
        public decimal DiscountPercentage => OriginalPrice > 0 ? (OriginalPrice - LivestreamPrice) / OriginalPrice * 100 : 0;
    }
}