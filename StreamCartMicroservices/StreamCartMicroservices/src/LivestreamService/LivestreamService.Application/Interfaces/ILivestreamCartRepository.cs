using LivestreamService.Domain.Entities;
using Shared.Common.Data.Interfaces;
namespace LivestreamService.Application.Interfaces
{
    public interface ILivestreamCartRepository : IGenericRepository<LivestreamCart>
    {
        /// <summary>
        /// Lấy cart của viewer trong livestream cụ thể
        /// </summary>
        Task<LivestreamCart?> GetByLivestreamAndViewerAsync(Guid livestreamId, Guid viewerId);

        /// <summary>
        /// Lấy cart với đầy đủ items
        /// </summary>
        Task<LivestreamCart?> GetWithItemsAsync(Guid cartId);

        /// <summary>
        /// Lấy các cart đã hết hạn
        /// </summary>
        Task<IEnumerable<LivestreamCart>> GetExpiredCartsAsync();

        /// <summary>
        /// Xóa các cart đã hết hạn
        /// </summary>
        Task<int> CleanupExpiredCartsAsync();

        /// <summary>
        /// Đếm số cart active trong livestream
        /// </summary>
        Task<int> CountActiveCartsInLivestreamAsync(Guid livestreamId);
    }

    public interface ILivestreamCartItemRepository : IGenericRepository<LivestreamCartItem>
    {
        /// <summary>
        /// Lấy cart items theo cart ID
        /// </summary>
        Task<IEnumerable<LivestreamCartItem>> GetByCartIdAsync(Guid cartId);

        /// <summary>
        /// Tìm cart item cụ thể trong cart
        /// </summary>
        Task<LivestreamCartItem?> FindByCartAndProductAsync(Guid cartId, Guid livestreamProductId, string? variantId = null);

        /// <summary>
        /// Lấy cart items theo livestream product ID
        /// </summary>
        Task<IEnumerable<LivestreamCartItem>> GetByLivestreamProductIdAsync(Guid livestreamProductId);

        /// <summary>
        /// Xóa cart item
        /// </summary>
        Task DeleteCartItemAsync(Guid cartItemId);

        /// <summary>
        /// Cập nhật stock cho tất cả cart items của một livestream product
        /// </summary>
        Task UpdateStockForLivestreamProductAsync(Guid livestreamProductId, int newStock);

    }
}