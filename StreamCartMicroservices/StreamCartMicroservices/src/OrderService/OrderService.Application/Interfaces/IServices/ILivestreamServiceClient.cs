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
    }
}