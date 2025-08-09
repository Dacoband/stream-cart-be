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
    // ✅ REMOVED [Authorize] for Docker compatibility - handle auth manually
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
        // ✅ CRITICAL: Override OnConnectedAsync for Docker compatibility
        // ✅ CRITICAL: Override OnConnectedAsync for Docker compatibility
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;

            try
            {
                _logger.LogInformation("🔧 SignalRChatHub connection attempt from {ConnectionId}", connectionId);

                // ✅ Log more details about the connection
                var httpContext = Context.GetHttpContext();
                if (httpContext != null)
                {
                    _logger.LogInformation("🔧 Connection Headers: {Headers}",
                        string.Join(", ", httpContext.Request.Headers.Select(h => $"{h.Key}={h.Value}") ?? new string[0]));
                    _logger.LogInformation("🔧 Connection Query: {Query}",
                        string.Join(", ", httpContext.Request.Query.Select(q => $"{q.Key}={q.Value}") ?? new string[0]));
                    _logger.LogInformation("🔧 Protocol: {Protocol}, Scheme: {Scheme}, Host: {Host}",
                        httpContext.Request.Protocol, httpContext.Request.Scheme, httpContext.Request.Host);
                }

                // ✅ Check authentication but don't fail if not authenticated
                var isAuthenticated = Context.User?.Identity?.IsAuthenticated == true;
                _logger.LogInformation("🔧 User authentication status: {IsAuth} for {ConnectionId}",
                    isAuthenticated, connectionId);

                if (isAuthenticated)
                {
                    var userId = GetCurrentUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        _userConnectionMap.TryAdd(connectionId, userId);
                        _logger.LogInformation("✅ User {UserId} authenticated and mapped to {ConnectionId}",
                            userId, connectionId);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Authenticated user but failed to get UserId for {ConnectionId}", connectionId);
                    }
                }
                else
                {
                    _logger.LogInformation("ℹ️ Anonymous connection allowed: {ConnectionId}", connectionId);
                }

                // ✅ CRITICAL: Always complete handshake with detailed logging
                _logger.LogInformation("🔧 Attempting to complete base handshake for {ConnectionId}", connectionId);
                Console.WriteLine($"[HUB] User: {Context.User?.Identity?.Name}, Auth: {Context.User?.Identity?.IsAuthenticated}");

                await base.OnConnectedAsync();

                _logger.LogInformation("✅ SignalRChatHub handshake completed successfully for {ConnectionId}", connectionId);

                // ✅ Send immediate confirmation to client
                try
                {
                    await Clients.Caller.SendAsync("Connected", new
                    {
                        ConnectionId = connectionId,
                        Status = "Connected",
                        IsAuthenticated = isAuthenticated,
                        UserId = GetCurrentUserId(),
                        Timestamp = DateTime.UtcNow,
                        Message = "SignalR connection established successfully"
                    });

                    _logger.LogInformation("✅ Confirmation message sent to {ConnectionId}", connectionId);
                }
                catch (Exception confirmEx)
                {
                    _logger.LogWarning(confirmEx, "⚠️ Failed to send confirmation to {ConnectionId}, but handshake OK", connectionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SignalRChatHub OnConnectedAsync for {ConnectionId}", connectionId);

                // ✅ CRITICAL: Try to complete handshake even on error
                try
                {
                    _logger.LogInformation("🔧 Attempting recovery base handshake for {ConnectionId}", connectionId);
                    await base.OnConnectedAsync();
                    _logger.LogInformation("✅ Recovery handshake completed for {ConnectionId}", connectionId);
                }
                catch (Exception baseEx)
                {
                    _logger.LogError(baseEx, "❌ CRITICAL: Failed to complete handshake for {ConnectionId}", connectionId);
                    // ✅ Don't throw - let SignalR handle gracefully
                }
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            try
            {
                _userConnectionMap.TryRemove(connectionId, out var userId);

                _logger.LogInformation("🔧 User {UserId} disconnected from {ConnectionId}. Exception: {Exception}",
                    userId ?? "Anonymous", connectionId, exception?.Message ?? "None");

                await base.OnDisconnectedAsync(exception);

                _logger.LogInformation("✅ Disconnection cleanup completed for {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in OnDisconnectedAsync for {ConnectionId}", connectionId);

                try
                {
                    await base.OnDisconnectedAsync(exception);
                }
                catch (Exception baseEx)
                {
                    _logger.LogError(baseEx, "❌ Critical: Failed to complete disconnection for {ConnectionId}", connectionId);
                }
            }
        }

        // ✅ All hub methods with manual authentication checks
        public async Task JoinDirectChatRoom(string chatRoomId)
        {
            if (!IsUserAuthenticated())
            {
                _logger.LogWarning("Unauthenticated user attempted to join chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Authentication required");
                return;
            }

            try
            {
                var userId = GetCurrentUserId();
                await Groups.AddToGroupAsync(Context.ConnectionId, $"chatroom_{chatRoomId}");

                _logger.LogInformation("User {UserId} joined chat room {ChatRoomId}", userId, chatRoomId);

                await Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("UserJoined", new { UserId = userId, Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to join chat room");
            }
        }

        public async Task LeaveDirectChatRoom(string chatRoomId)
        {
            if (!IsUserAuthenticated()) return;

            try
            {
                var userId = GetCurrentUserId();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatroom_{chatRoomId}");

                _logger.LogInformation("User {UserId} left chat room {ChatRoomId}", userId, chatRoomId);

                await Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("UserLeft", new { UserId = userId, Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task SendMessageToChatRoom(string chatRoomId, string message)
        {
            if (!IsUserAuthenticated())
            {
                await Clients.Caller.SendAsync("Error", "Authentication required");
                return;
            }

            try
            {
                var userName = GetCurrentUserName();
                var userId = GetCurrentUserId();

                _logger.LogInformation("User {UserId} sent message to chat room {ChatRoomId}", userId, chatRoomId);

                await Clients.Group($"chatroom_{chatRoomId}")
                    .SendAsync("ReceiveChatMessage", new
                    {
                        SenderId = userId,
                        SenderName = userName,
                        Message = message,
                        Timestamp = DateTime.UtcNow
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chat room {ChatRoomId}", chatRoomId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        public async Task SendMessageToLivestream(string livestreamId, string message)
        {
            if (!IsUserAuthenticated())
            {
                await Clients.Caller.SendAsync("Error", "Authentication required");
                return;
            }

            try
            {
                var userName = GetCurrentUserName();
                var userId = GetCurrentUserId();

                _logger.LogInformation("User {UserId} sent message to livestream {LivestreamId}", userId, livestreamId);

                await Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("ReceiveLivestreamMessage", new
                    {
                        SenderId = userId,
                        SenderName = userName,
                        Message = message,
                        Timestamp = DateTime.UtcNow
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to livestream {LivestreamId}", livestreamId);
                await Clients.Caller.SendAsync("Error", "Failed to send message");
            }
        }

        public async Task SetTypingStatus(string chatRoomId, bool isTyping)
        {
            if (!IsUserAuthenticated()) return;

            try
            {
                await Clients.OthersInGroup($"chatroom_{chatRoomId}")
                    .SendAsync("UserTyping", new
                    {
                        UserId = GetCurrentUserId(),
                        IsTyping = isTyping
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting typing status for chat room {ChatRoomId}", chatRoomId);
            }
        }

        public async Task JoinLivestreamChatRoom(string livestreamId)
        {
            if (!IsUserAuthenticated())
            {
                await Clients.Caller.SendAsync("Error", "Authentication required");
                return;
            }

            try
            {
                var userId = GetCurrentUserId();
                await Groups.AddToGroupAsync(Context.ConnectionId, $"livestream_{livestreamId}");

                _logger.LogInformation("User {UserId} joined livestream {LivestreamId} chat", userId, livestreamId);

                await Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("UserJoined", new { UserId = userId, Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining livestream chat room {LivestreamId}", livestreamId);
            }
        }

        public async Task LeaveLivestreamChatRoom(string livestreamId)
        {
            if (!IsUserAuthenticated()) return;

            try
            {
                var userId = GetCurrentUserId();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"livestream_{livestreamId}");

                _logger.LogInformation("User {UserId} left livestream {LivestreamId} chat", userId, livestreamId);

                await Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("UserLeft", new { UserId = userId, Timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving livestream chat room {LivestreamId}", livestreamId);
            }
        }

        // ✅ Helper methods
        private bool IsUserAuthenticated()
        {
            try
            {
                return Context.User?.Identity?.IsAuthenticated == true;
            }
            catch
            {
                return false;
            }
        }

        private string? GetCurrentUserId()
        {
            try
            {
                return Context.User?.FindFirst("id")?.Value
                    ?? Context.User?.FindFirst("sub")?.Value
                    ?? Context.User?.FindFirst("nameid")?.Value
                    ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user ID for connection {ConnectionId}", Context.ConnectionId);
                return null;
            }
        }

        private string? GetCurrentUserName()
        {
            try
            {
                return Context.User?.FindFirst("unique_name")?.Value
                       ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value
                       ?? "Anonymous";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting username for connection {ConnectionId}", Context.ConnectionId);
                return "Anonymous";
            }
        }
    }
}