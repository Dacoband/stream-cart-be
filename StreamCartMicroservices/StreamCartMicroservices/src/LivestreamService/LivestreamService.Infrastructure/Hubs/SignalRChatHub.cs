using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
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
        private readonly IStreamViewRepository _streamViewRepository;
        private readonly ILivestreamRepository _livestreamRepository;

        private static readonly ConcurrentDictionary<string, (Guid livestreamId, Guid userId, string role, DateTime startTime)> _activeViewers = new();
        private readonly IAccountServiceClient _accountServiceClient;

        public SignalRChatHub(ILogger<SignalRChatHub> logger, ICurrentUserService currentUserService, IStreamViewRepository streamViewRepository, ILivestreamRepository livestreamRepository, IAccountServiceClient accountServiceClient)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _streamViewRepository = streamViewRepository;
            _livestreamRepository = livestreamRepository;
            _accountServiceClient = accountServiceClient;
        }

        public async Task StartViewingLivestream(string livestreamId)
        {
            if (!IsUserAuthenticated())
            {
                await Clients.Caller.SendAsync("Error", "Authentication required");
                return;
            }

            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    await Clients.Caller.SendAsync("Error", "User ID not found");
                    return;
                }

                var userGuid = Guid.Parse(userId);
                var livestreamGuid = Guid.Parse(livestreamId);

                // ✅ GET USER ROLE FIRST
                var userRole = await GetUserRoleAsync(userGuid);
                var startTime = DateTime.UtcNow;
                // ✅ FIX: Store the connection mapping WITH role information
                _activeViewers[Context.ConnectionId] = (livestreamGuid, userGuid, userRole,startTime);
                await CreateStreamViewRecordAsync(livestreamGuid, userGuid);

                _logger.LogInformation("User {UserId} with role {Role} started viewing livestream {LivestreamId}",
                    userId, userRole, livestreamId);

                // Add to the livestream group for targeted updates
                await Groups.AddToGroupAsync(Context.ConnectionId, $"livestream_viewers_{livestreamId}");

                // ✅ Update max customer viewer count
                await UpdateMaxCustomerViewerCount(livestreamGuid);

                // Broadcast updated viewer stats
                await BroadcastViewerStats(livestreamGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting to view livestream {LivestreamId}", livestreamId);
                await Clients.Caller.SendAsync("Error", "Failed to start viewing livestream");
            }
        }

        public async Task StopViewingLivestream(string livestreamId)
        {
            if (!IsUserAuthenticated())
                return;

            try
            {
                var userId = GetCurrentUserId();
                var livestreamGuid = Guid.Parse(livestreamId);

                // ✅ FIX: Remove the connection mapping and get the viewer info
                if (_activeViewers.TryRemove(Context.ConnectionId, out var viewerInfo))
                {
                    // ✅ End the StreamView record in database
                    await EndStreamViewRecordAsync(livestreamGuid, viewerInfo.userId);

                    var duration = DateTime.UtcNow - viewerInfo.startTime;
                    _logger.LogInformation("User {UserId} with role {Role} stopped viewing livestream {LivestreamId} after {Duration}",
                        userId, viewerInfo.role, livestreamId, duration);
                }

                // Remove from the livestream group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"livestream_viewers_{livestreamId}");

                // Broadcast updated viewer stats
                await BroadcastViewerStats(livestreamGuid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping viewing livestream {LivestreamId}", livestreamId);
            }
        }

        private async Task UpdateMaxCustomerViewerCount(Guid livestreamId)
        {
            try
            {
                // ✅ FIX: Now we can access the role property correctly
                var currentCustomerViewerCount = _activeViewers
                    .Count(kv => kv.Value.livestreamId == livestreamId &&
                                kv.Value.role.Equals("Customer", StringComparison.OrdinalIgnoreCase));

                // Get the livestream entity
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamId.ToString());
                if (livestream == null)
                {
                    _logger.LogWarning("Livestream {LivestreamId} not found when updating max customer viewer count", livestreamId);
                    return;
                }

                // Check if current customer count exceeds the previous maximum
                var currentMaxViewer = livestream.MaxViewer ?? 0;
                if (currentCustomerViewerCount > currentMaxViewer)
                {
                    // Update the max viewer count with customer count
                    livestream.SetMaxViewer(currentCustomerViewerCount, "system");
                    await _livestreamRepository.ReplaceAsync(livestreamId.ToString(), livestream);

                    _logger.LogInformation("Updated MaxViewer for livestream {LivestreamId} from {OldMax} to {NewMax} (Customer viewers only)",
                        livestreamId, currentMaxViewer, currentCustomerViewerCount);

                    // Notify about new customer viewer record
                    await Clients.Group($"livestream_{livestreamId}")
                        .SendAsync("MaxCustomerViewerUpdated", new
                        {
                            LivestreamId = livestreamId,
                            NewMaxCustomerViewer = currentCustomerViewerCount,
                            PreviousMax = currentMaxViewer,
                            ViewerType = "Customer",
                            Timestamp = DateTime.UtcNow,
                            Message = $"🎉 Kỷ lục mới! Có {currentCustomerViewerCount} khách hàng đang xem cùng lúc!"
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating max customer viewer count for livestream {LivestreamId}", livestreamId);
            }
        }

        private async Task UpdateMaxViewerCount(Guid livestreamId)
        {
            try
            {
                // Count current active viewers for this livestream
                var currentViewerCount = _activeViewers
                    .Count(kv => kv.Value.livestreamId == livestreamId);

                // Get the livestream entity
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamId.ToString());
                if (livestream == null)
                {
                    _logger.LogWarning("Livestream {LivestreamId} not found when updating max viewer count", livestreamId);
                    return;
                }

                // Check if current count exceeds the previous maximum
                var currentMaxViewer = livestream.MaxViewer ?? 0;
                if (currentViewerCount > currentMaxViewer)
                {
                    // Update the max viewer count
                    livestream.SetMaxViewer(currentViewerCount, "system");
                    await _livestreamRepository.ReplaceAsync(livestreamId.ToString(), livestream);

                    _logger.LogInformation("Updated MaxViewer for livestream {LivestreamId} from {OldMax} to {NewMax}",
                        livestreamId, currentMaxViewer, currentViewerCount);

                    // Notify about new record
                    await Clients.Group($"livestream_{livestreamId}")
                        .SendAsync("MaxViewerUpdated", new
                        {
                            LivestreamId = livestreamId,
                            NewMaxViewer = currentViewerCount,
                            PreviousMax = currentMaxViewer,
                            Timestamp = DateTime.UtcNow
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating max viewer count for livestream {LivestreamId}", livestreamId);
            }
        }

        private async Task BroadcastViewerStats(Guid livestreamId)
        {
            try
            {
                // Count active viewers by role
                var roleCounts = new Dictionary<string, int>();
                var viewerConnections = _activeViewers
                    .Where(kv => kv.Value.livestreamId == livestreamId)
                    .ToList();

                foreach (var connection in viewerConnections)
                {
                    // ✅ FIX: Now we can access the role property correctly
                    var role = connection.Value.role;

                    // Update role counts
                    if (roleCounts.ContainsKey(role))
                        roleCounts[role]++;
                    else
                        roleCounts[role] = 1;
                }

                // ✅ GET MAX VIEWER FROM DATABASE (Customer focused)
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamId.ToString());
                var maxCustomerViewer = livestream?.MaxViewer ?? 0;

                // ✅ Count current customer viewers specifically
                var currentCustomerViewers = roleCounts.GetValueOrDefault("Customer", 0);

                // Create enhanced statistics object
                var viewerStats = new
                {
                    LivestreamId = livestreamId,
                    TotalViewers = viewerConnections.Count,
                    CustomerViewers = currentCustomerViewers, // ✅ Current customer count
                    MaxCustomerViewer = maxCustomerViewer, // ✅ Historical max customer viewers
                    ViewersByRole = roleCounts,
                    IsNewRecord = currentCustomerViewers == maxCustomerViewer && maxCustomerViewer > 0,
                    Timestamp = DateTime.UtcNow
                };

                // Broadcast to all clients in the livestream
                await Clients.Group($"livestream_{livestreamId}")
                    .SendAsync("ReceiveViewerStats", viewerStats);

                // Also broadcast to specific viewer stats group
                await Clients.Group($"livestream_viewers_{livestreamId}")
                    .SendAsync("ReceiveViewerStats", viewerStats);

                _logger.LogInformation("📊 Viewer stats for livestream {LivestreamId}: {TotalViewers} total, {CustomerViewers} customers, {MaxCustomer} max customers (record)",
                    livestreamId, viewerConnections.Count, currentCustomerViewers, maxCustomerViewer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting viewer stats for livestream {LivestreamId}", livestreamId);
            }
        }
        private async Task CreateStreamViewRecordAsync(Guid livestreamId, Guid userId)
        {
            try
            {
                // Check if user already has an active view
                var existingView = await _streamViewRepository.GetActiveViewByUserAsync(livestreamId, userId);
                if (existingView != null)
                {
                    _logger.LogInformation("User {UserId} already has active view for livestream {LivestreamId}", userId, livestreamId);
                    return;
                }

                // Create new StreamView record
                var streamView = new StreamView(livestreamId, userId, DateTime.UtcNow, "signalr-hub");
                await _streamViewRepository.InsertAsync(streamView);

                _logger.LogInformation("✅ Created StreamView record for user {UserId} in livestream {LivestreamId}", userId, livestreamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating StreamView record for user {UserId} in livestream {LivestreamId}", userId, livestreamId);
            }
        }
        private async Task EndStreamViewRecordAsync(Guid livestreamId, Guid userId)
        {
            try
            {
                var activeView = await _streamViewRepository.GetActiveViewByUserAsync(livestreamId, userId);
                if (activeView != null)
                {
                    activeView.EndView(DateTime.UtcNow, "signalr-hub");
                    await _streamViewRepository.ReplaceAsync(activeView.Id.ToString(), activeView);

                    var duration = DateTime.UtcNow - activeView.StartTime;
                    _logger.LogInformation("✅ Ended StreamView record for user {UserId} in livestream {LivestreamId}, duration: {Duration}",
                        userId, livestreamId, duration);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending StreamView record for user {UserId} in livestream {LivestreamId}", userId, livestreamId);
            }
        }

        private async Task<string> GetUserRoleAsync(Guid userId)
        {
            try
            {
                var userAccount = await _accountServiceClient.GetAccountByIdAsync(userId);
                if (userAccount != null)
                {
                    if (userAccount.Role is int roleInt)
                    {
                        // Convert int role to string (assuming 1=Admin, 2=Seller, 3=Customer)
                        return roleInt switch
                        {
                            1 => "Admin",
                            2 => "Seller",
                            3 => "Customer",
                            _ => "Unknown"
                        };
                    }
                    else
                    {
                        // Role is enum, convert to string
                        return userAccount.Role.ToString();
                    }
                }
                return "Unknown";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine role for user {UserId}", userId);
                return "Unknown";
            }
        }

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
                // ✅ FIX: Now we can access viewingInfo with role information
                if (_activeViewers.TryRemove(connectionId, out var viewingInfo))
                {
                    await EndStreamViewRecordAsync(viewingInfo.livestreamId, viewingInfo.userId);
                    var duration = DateTime.UtcNow - viewingInfo.startTime;

                    _logger.LogInformation("User {UserId} with role {Role} disconnected from livestream {LivestreamId} after {Duration}",
                        viewingInfo.userId, viewingInfo.role, viewingInfo.livestreamId, duration);

                    // Broadcast updated stats
                    await BroadcastViewerStats(viewingInfo.livestreamId);
                }
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
        private string[] GetUserRoles()
        {
            try
            {
                var rolesClaims = Context.User?.FindAll("role").Select(c => c.Value).ToArray();
                return rolesClaims ?? new string[] { "Viewer" };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user roles for connection {ConnectionId}", Context.ConnectionId);
                return new string[] { "Unknown" };
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