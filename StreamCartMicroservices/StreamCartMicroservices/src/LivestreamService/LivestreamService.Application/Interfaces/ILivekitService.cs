using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface ILivekitService
    {
        /// <summary>
        /// Creates a new room in LiveKit
        /// </summary>
        /// <param name="roomName">Room name</param>
        /// <returns>Room ID</returns>
        Task<string> CreateRoomAsync(string roomName);

        /// <summary>
        /// Deletes a room from LiveKit
        /// </summary>
        /// <param name="roomName">Room name to delete</param>
        /// <returns>True if successful</returns>
        Task<bool> DeleteRoomAsync(string roomName);

        /// <summary>
        /// Generates a join token for a LiveKit room
        /// </summary>
        /// <param name="roomName">Room name</param>
        /// <param name="participantName">Participant name</param>
        /// <param name="canPublish">Whether the participant can publish</param>
        /// <returns>Join token</returns>
        Task<string> GenerateJoinTokenAsync(string roomName, string participantName, bool canPublish);

        /// <summary>
        /// Gets active participants in a room
        /// </summary>
        /// <param name="roomName">Room name</param>
        /// <returns>Count of participants</returns>
        Task<int> GetParticipantCountAsync(string roomName);


        /// <summary>
        /// Tạo chat room dành riêng cho chat giữa shop và customer
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Room name for chat</returns>
        Task<string> CreateChatRoomAsync(Guid shopId, Guid customerId);

        /// <summary>
        /// Generate token cho chat room (chỉ data channel, không video/audio)
        /// </summary>
        /// <param name="roomName">Room name</param>
        /// <param name="participantName">Participant name</param>
        /// <param name="isShop">True nếu là shop, false nếu là customer</param>
        /// <returns>Join token for chat only</returns>
        Task<string> GenerateChatTokenAsync(string roomName, string participantName, bool isShop = false);

        /// <summary>
        /// Gửi tin nhắn qua LiveKit data channel
        /// </summary>
        /// <param name="roomName">Room name</param>
        /// <param name="senderId">Sender ID</param>
        /// <param name="message">Message content</param>
        /// <returns>True if successful</returns>
        Task<bool> SendDataMessageAsync(string roomName, string senderId, object message);

        /// <summary>
        /// Kiểm tra room có đang hoạt động không
        /// </summary>
        /// <param name="roomName">Room name</param>
        /// <returns>True if room is active</returns>
        Task<bool> IsRoomActiveAsync(string roomName);
    }
}