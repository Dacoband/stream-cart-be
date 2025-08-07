using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.User;
using System;
using System.Collections.Concurrent;
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
            await Groups.AddToGroupAsync(Context.ConnectionId, $"livestream_sr_{livestreamId}");
            _logger.LogInformation("User {UserId} joined SignalR livestream {LivestreamId} chat",
                _currentUserService.GetUserId(), livestreamId);

            await Clients.Group($"livestream_sr_{livestreamId}")
                .SendAsync("UserJoined", new { UserId = _currentUserService.GetUserId(), Timestamp = DateTime.UtcNow });
        }

        public async Task LeaveLivestreamChatRoom(string livestreamId)
        {
            var userId = _currentUserService.GetUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"livestream_sr_{livestreamId}");
            _logger.LogInformation("User {UserId} left SignalR livestream {LivestreamId} chat",
                userId, livestreamId);

            await Clients.Group($"livestream_sr_{livestreamId}")
                .SendAsync("UserLeft", new { UserId = userId, Timestamp = DateTime.UtcNow });
        }
        public async Task JoinDirectChatRoom(string chatRoomId)
        {
            var userId = _currentUserService.GetUserId();
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chatroom_sr_{chatRoomId}");
            _logger.LogInformation("User {UserId} joined SignalR chat room {ChatRoomId}",
                userId, chatRoomId);

            await Clients.Group($"chatroom_sr_{chatRoomId}")
                .SendAsync("UserJoined", new { UserId = userId, Timestamp = DateTime.UtcNow });
        }

        public async Task LeaveDirectChatRoom(string chatRoomId)
        {
            var userId = _currentUserService.GetUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatroom_sr_{chatRoomId}");
            _logger.LogInformation("User {UserId} left SignalR chat room {ChatRoomId}",
                userId, chatRoomId);

            await Clients.Group($"chatroom_sr_{chatRoomId}")
                .SendAsync("UserLeft", new { UserId = userId, Timestamp = DateTime.UtcNow });
        }
        public async Task SendMessageToLivestream(string livestreamId, string message)
        {
            var userName = _currentUserService.GetUsername() ?? "Anonymous";
            var userId = _currentUserService.GetUserId();

            _logger.LogInformation("User {UserId} sent message to livestream {LivestreamId}: {Message}",
                userId, livestreamId, message);

            await Clients.Group($"livestream_sr_{livestreamId}")
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
            var userName = _currentUserService.GetUsername() ?? "Anonymous";
            var userId = _currentUserService.GetUserId();

            _logger.LogInformation("User {UserId} sent message to chat room {ChatRoomId}: {Message}",
                userId, chatRoomId, message);

            await Clients.Group($"chatroom_sr_{chatRoomId}")
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
            await Clients.OthersInGroup($"chatroom_sr_{chatRoomId}")
                .SendAsync("UserTyping", new
                {
                    UserId = _currentUserService.GetUserId(),
                    IsTyping = isTyping
                });
        }

        public override async Task OnConnectedAsync()
        {
            var userId = _currentUserService.GetUserId();
            _userConnectionMap.TryAdd(Context.ConnectionId, userId.ToString());

            _logger.LogInformation("User {UserId} connected to SignalR chat hub", userId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _currentUserService.GetUserId();
            _userConnectionMap.TryRemove(Context.ConnectionId, out _);

            _logger.LogInformation("User {UserId} disconnected from SignalR chat hub", userId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}