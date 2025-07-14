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
    }
}