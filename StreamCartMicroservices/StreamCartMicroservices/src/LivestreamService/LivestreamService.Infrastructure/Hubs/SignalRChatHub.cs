using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.User;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Hubs
{
    [Authorize]
    public class SignalRChatHub : Hub
    {
        private readonly ILogger<SignalRChatHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _userConnectionMap = new();
        private readonly ICurrentUserService _currentUserService;

        public SignalRChatHub(ILogger<SignalRChatHub> logger, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }
        public async Task JoinLivestreamChatRoom(string livestreamId)
        {
            var userid = GetCurrentUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"livestream_{livestreamId}");
            _logger.LogInformation("User {UserId} joined SignalR livestream {LivestreamId} chat",
                userid, livestreamId);

            await Clients.Group($"livestream_{livestreamId}")
                .SendAsync("UserJoined", new { UserId = userid, Timestamp = DateTime.UtcNow });
        }

        public async Task LeaveLivestreamChatRoom(string livestreamId)
        {
            var userId = GetCurrentUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"livestream_{livestreamId}");
            _logger.LogInformation("User {UserId} left SignalR livestream {LivestreamId} chat",
                userId, livestreamId);

            await Clients.Group($"livestream_{livestreamId}")
                .SendAsync("UserLeft", new { UserId = userId, Timestamp = DateTime.UtcNow });
        }
        public async Task JoinDirectChatRoom(string chatRoomId)
        {
            var userId = GetCurrentUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chatroom_{chatRoomId}");
            _logger.LogInformation("User {UserId} joined SignalR chat room {ChatRoomId}",
                userId, chatRoomId);

            await Clients.Group($"chatroom_{chatRoomId}")
                .SendAsync("UserJoined", new { UserId = userId, Timestamp = DateTime.UtcNow });
        }

        public async Task LeaveDirectChatRoom(string chatRoomId)
        {
            var userId = GetCurrentUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatroom_{chatRoomId}");
            _logger.LogInformation("User {UserId} left SignalR chat room {ChatRoomId}",
                userId, chatRoomId);

            await Clients.Group($"chatroom_{chatRoomId}")
                .SendAsync("UserLeft", new { UserId = userId, Timestamp = DateTime.UtcNow });
        }
        public async Task SendMessageToLivestream(string livestreamId, string message)
        {
            var userName = GetCurrentUserName();
            var userId = GetCurrentUserId();

            _logger.LogInformation("User {UserId} sent message to livestream {LivestreamId}: {Message}",
                userId, livestreamId, message);

            await Clients.Group($"livestream_{livestreamId}")
                .SendAsync("ReceiveLivestreamMessage", new
                {
                    SenderId = userId,
                    SenderName = userName,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task SendMessageToChatRoom(string chatRoomId, string message)
        {
            var userName = GetCurrentUserName();
            var userId = GetCurrentUserId();

            _logger.LogInformation("User {UserId} sent message to chat room {ChatRoomId}: {Message}",
                userId, chatRoomId, message);

            await Clients.Group($"chatroom_{chatRoomId}")
                .SendAsync("ReceiveChatMessage", new
                {
                    SenderId = userId,
                    SenderName = userName,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });
        }
        public async Task SetTypingStatus(string chatRoomId, bool isTyping)
        {
            await Clients.OthersInGroup($"chatroom_{chatRoomId}")
                .SendAsync("UserTyping", new
                {
                    UserId = GetCurrentUserId(),
                    IsTyping = isTyping
                });
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Failed to get user ID for connection {ConnectionId}", Context.ConnectionId);
                await base.OnConnectedAsync();
                return;
            }
            _userConnectionMap.TryAdd(Context.ConnectionId, userId.ToString());

            _logger.LogInformation("User {UserId} connected to SignalR chat hub", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetCurrentUserId();
            _userConnectionMap.TryRemove(Context.ConnectionId, out _);

            _logger.LogInformation("User {UserId} disconnected from SignalR chat hub", userId);
            await base.OnDisconnectedAsync(exception);
        }
        private string? GetCurrentUserId()
        {
            return Context.User?.FindFirst("id")?.Value
                ?? Context.User?.FindFirst("sub")?.Value
                ?? Context.User?.FindFirst("nameid")?.Value
                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        private string? GetCurrentUserName()
        {
            return Context.User?.FindFirst("unique_name")?.Value
             ;
        }
    }
}