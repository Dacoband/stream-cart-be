using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        // Livestream Chat Methods
        public async Task JoinLivestreamChat(string livestreamId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"livestream_{livestreamId}");
            _logger.LogInformation("User {UserId} joined livestream {LivestreamId} chat",
                Context.UserIdentifier, livestreamId);
        }

        public async Task LeaveLivestreamChat(string livestreamId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"livestream_{livestreamId}");
            _logger.LogInformation("User {UserId} left livestream {LivestreamId} chat",
                Context.UserIdentifier, livestreamId);
        }

        // Direct Chat Methods
        public async Task JoinChatRoom(string chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chatroom_{chatRoomId}");
            _logger.LogInformation("User {UserId} joined chat room {ChatRoomId}",
                Context.UserIdentifier, chatRoomId);
        }

        public async Task LeaveChatRoom(string chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatroom_{chatRoomId}");
            _logger.LogInformation("User {UserId} left chat room {ChatRoomId}",
                Context.UserIdentifier, chatRoomId);
        }

        // User Status Methods
        public async Task SetTyping(string chatRoomId, bool isTyping)
        {
            await Clients.OthersInGroup($"chatroom_{chatRoomId}")
                .SendAsync("UserTyping", new
                {
                    UserId = Context.UserIdentifier,
                    IsTyping = isTyping
                });
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("User {UserId} connected to chat hub", Context.UserIdentifier);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("User {UserId} disconnected from chat hub", Context.UserIdentifier);
            await base.OnDisconnectedAsync(exception);
        }
    }
}