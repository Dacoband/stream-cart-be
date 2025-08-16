using LivestreamService.Application.Commands.LiveStreamService;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries;
using LivestreamService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.User;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using static MassTransit.ValidationResultExtensions;

namespace LivestreamService.Infrastructure.Hubs
{
    public class SignalRChatHub : Hub
    {
        private readonly ILogger<SignalRChatHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _userConnectionMap = new();
        private readonly ICurrentUserService _currentUserService;
        private readonly IStreamViewRepository _streamViewRepository;
        private readonly ILivestreamRepository _livestreamRepository;

        private static readonly ConcurrentDictionary<string, (Guid livestreamId, Guid userId, string role, DateTime startTime)> _activeViewers = new();
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IMediator _mediator; // ✅ ADD: For handling commands

        public SignalRChatHub(ILogger<SignalRChatHub> logger, ICurrentUserService currentUserService, IStreamViewRepository streamViewRepository, ILivestreamRepository livestreamRepository, IAccountServiceClient accountServiceClient,IMediator mediator)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _streamViewRepository = streamViewRepository;
            _livestreamRepository = livestreamRepository;
            _accountServiceClient = accountServiceClient;
            _mediator = mediator;
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
        /// <summary>
        /// Real-time cập nhật stock sản phẩm trong livestream
        /// </summary>
        /// <param name="livestreamId">ID của livestream</param>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="variantId">ID của variant (optional)</param>
        /// <param name="newStock">Số lượng stock mới</param>
        public async Task UpdateProductStock(string livestreamId, string productId, string? variantId, int newStock)
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

                // Execute update command
                var command = new UpdateStockCommand
                {
                    LivestreamId = Guid.Parse(livestreamId),
                    ProductId = productId,
                    VariantId = variantId ?? string.Empty,
                    Stock = newStock,
                    SellerId = userGuid
                };
                var result = await _mediator.Send(command);

                if (result != null)
                {
                    // ✅ BROADCAST real-time update to all viewers
                    await Clients.Group($"livestream_viewers_{livestreamId}")
                        .SendAsync("ProductStockUpdated", new
                        {
                            LivestreamId = livestreamId,
                            ProductId = productId,
                            VariantId = variantId,
                            NewStock = newStock,
                            OriginalPrice = result.OriginalPrice,
                            Price = result.Price,
                            ProductName = result.ProductName,
                            UpdatedBy = userId,
                            Timestamp = DateTime.UtcNow,
                            Message = $"📦 Stock được cập nhật: {newStock} sản phẩm còn lại"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = "StockUpdate",
                        ProductId = productId,
                        NewStock = newStock,
                        OriginalPrice = result.OriginalPrice,
                        Price = result.Price,
                        Message = "Cập nhật stock thành công"
                    });

                    _logger.LogInformation("✅ Real-time stock update: Product {ProductId} in livestream {LivestreamId} updated to {NewStock}",
                        productId, livestreamId, newStock);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to update product stock");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product stock real-time");
                await Clients.Caller.SendAsync("Error", $"Stock update failed: {ex.Message}");
            }
        }
        /// <summary>
        /// Real-time pin/unpin sản phẩm trong livestream
        /// </summary>
        /// <param name="livestreamId">ID của livestream</param>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="variantId">ID của variant (optional)</param>
        /// <param name="isPin">true để pin, false để unpin</param>
        public async Task PinProduct(string livestreamId, string productId, string? variantId, bool isPin)
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

                // Execute pin command
                var command = new PinProductCommand
                {
                    LivestreamId = Guid.Parse(livestreamId),
                    ProductId = productId,
                    VariantId = variantId ?? string.Empty,
                    IsPin = isPin,
                    SellerId = userGuid
                };
                var result = await _mediator.Send(command);

                if (result != null)
                {
                    // ✅ BROADCAST real-time pin/unpin to all viewers
                    await Clients.Group($"livestream_viewers_{livestreamId}")
                        .SendAsync("ProductPinStatusChanged", new
                        {
                            LivestreamId = livestreamId,
                            ProductId = productId,
                            VariantId = variantId,
                            IsPin = isPin,
                            OriginalPrice = result.OriginalPrice,
                            Price = result.Price,
                            Stock = result.Stock,
                            UpdatedBy = userId,
                            ProductName = result.ProductName,
                            Timestamp = DateTime.UtcNow,
                            Message = isPin ?
                                $"📌 {result.ProductName} đã được ghim!" :
                                $"📌 {result.ProductName} đã bỏ ghim!"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = isPin ? "ProductPinned" : "ProductUnpinned",
                        ProductId = productId,
                        ProductName = result.ProductName,
                        OriginalPrice = result.OriginalPrice,
                        Price = result.Price,
                        Message = isPin ? "Sản phẩm đã được ghim" : "Sản phẩm đã bỏ ghim"
                    });

                    _logger.LogInformation("✅ Real-time pin update: Product {ProductId} in livestream {LivestreamId} {Action}",
                        productId, livestreamId, isPin ? "pinned" : "unpinned");
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to update pin status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pin status real-time");
                await Clients.Caller.SendAsync("Error", $"Pin update failed: {ex.Message}");
            }
        }
        /// <summary>
        /// Real-time thêm sản phẩm vào livestream
        /// </summary>
        /// <param name="livestreamId">ID của livestream</param>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="variantId">ID của variant (optional)</param>
        /// <param name="price">Giá sản phẩm</param>
        /// <param name="stock">Số lượng tồn kho</param>
        /// <param name="isPin">Có pin ngay không</param>
        public async Task AddProductToLivestream(string livestreamId, string productId, string? variantId, decimal price, int stock, bool isPin = false)
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

                // Execute create command
                var command = new CreateLivestreamProductCommand
                {
                    LivestreamId = Guid.Parse(livestreamId),
                    ProductId = productId,
                    VariantId = variantId,
                    Price = price,
                    Stock = stock,
                    IsPin = isPin,
                    SellerId = userGuid
                };

                var result = await _mediator.Send(command);

                if (result != null)
                {
                    // ✅ BROADCAST real-time product addition to all viewers
                    await Clients.Group($"livestream_viewers_{livestreamId}")
                        .SendAsync("ProductAddedToLivestream", new
                        {
                            LivestreamId = livestreamId,
                            Product = result,
                            AddedBy = userId,
                            Timestamp = DateTime.UtcNow,
                            Message = $"🆕 Sản phẩm mới: {result.ProductName} đã được thêm vào livestream!"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = "ProductAdded",
                        Product = result,
                        Message = "Sản phẩm đã được thêm vào livestream"
                    });

                    _logger.LogInformation("✅ Real-time product add: Product {ProductId} added to livestream {LivestreamId}",
                        productId, livestreamId);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to add product to livestream");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product to livestream real-time");
                await Clients.Caller.SendAsync("Error", $"Add product failed: {ex.Message}");
            }
        }
        /// <summary>
        /// Real-time xóa sản phẩm khỏi livestream
        /// </summary>
        /// <param name="livestreamId">ID của livestream</param>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="variantId">ID của variant (optional)</param>
        public async Task RemoveProductFromLivestream(string livestreamId, string productId, string? variantId)
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

                // Execute delete command
                var command = new DeleteLivestreamProductCommand
                {
                    LivestreamId = Guid.Parse(livestreamId),
                    ProductId = productId,
                    VariantId = variantId ?? string.Empty,
                    SellerId = userGuid
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    // ✅ BROADCAST real-time product removal to all viewers
                    await Clients.Group($"livestream_viewers_{livestreamId}")
                        .SendAsync("ProductRemovedFromLivestream", new
                        {
                            LivestreamId = livestreamId,
                            ProductId = productId,
                            VariantId = variantId,
                            RemovedBy = userId,
                            Timestamp = DateTime.UtcNow,
                            Message = "🗑️ Một sản phẩm đã được gỡ khỏi livestream"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = "ProductRemoved",
                        ProductId = productId,
                        Message = "Sản phẩm đã được gỡ khỏi livestream"
                    });

                    _logger.LogInformation("✅ Real-time product remove: Product {ProductId} removed from livestream {LivestreamId}",
                        productId, livestreamId);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to remove product from livestream");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing product from livestream real-time");
                await Clients.Caller.SendAsync("Error", $"Remove product failed: {ex.Message}");
            }
        }
        /// <summary>
        /// Real-time lấy danh sách sản phẩm trong livestream
        /// </summary>
        /// <param name="livestreamId">ID của livestream</param>
        public async Task GetLivestreamProducts(string livestreamId)
        {
            try
            {
                var query = new GetLivestreamProductsQuery { LivestreamId = Guid.Parse(livestreamId) };
                var products = await _mediator.Send(query);

                await Clients.Caller.SendAsync("LivestreamProductsLoaded", new
                {
                    LivestreamId = livestreamId,
                    Products = products,
                    Timestamp = DateTime.UtcNow,
                    Count = products?.Count() ?? 0
                });

                _logger.LogInformation("✅ Real-time products loaded for livestream {LivestreamId}", livestreamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading livestream products");
                await Clients.Caller.SendAsync("Error", "Failed to load livestream products");
            }
        }
        /// <summary>
        /// Real-time lấy sản phẩm đã pin
        /// </summary>
        /// <param name="livestreamId">ID của livestream</param>
        public async Task GetPinnedProducts(string livestreamId)
        {
            try
            {
                var query = new GetPinnedProductsQuery { LivestreamId = Guid.Parse(livestreamId), Limit = 5 };
                var pinnedProducts = await _mediator.Send(query);

                await Clients.Group($"livestream_viewers_{livestreamId}")
                    .SendAsync("PinnedProductsUpdated", new
                    {
                        LivestreamId = livestreamId,
                        PinnedProducts = pinnedProducts,
                        Timestamp = DateTime.UtcNow,
                        Count = pinnedProducts?.Count() ?? 0
                    });

                _logger.LogInformation("✅ Real-time pinned products updated for livestream {LivestreamId}", livestreamId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading pinned products");
                await Clients.Caller.SendAsync("Error", "Failed to load pinned products");
            }
        }
        /// <summary>
        /// Broadcast stock update từ bên ngoài (ví dụ: khi có đơn hàng)
        /// </summary>
        /// <param name="livestreamId">ID của livestream</param>
        /// <param name="productId">ID của sản phẩm</param>
        /// <param name="variantId">ID của variant</param>
        /// <param name="newStock">Stock mới</param>
        /// <param name="reason">Lý do thay đổi</param>
        /// <param name="originalPrice">Giá gốc (optional)</param>
        /// <param name="currentPrice">Giá hiện tại (optional)</param>

        public async Task BroadcastStockChange(string livestreamId, string productId, string? variantId, int newStock, string reason,
            decimal? originalPrice = null, decimal? currentPrice = null, decimal? discountPercentage = null)
        {
            try
            {
                // ✅ BROADCAST to all viewers with price information
                var notification = new
                {
                    LivestreamId = livestreamId,
                    ProductId = productId,
                    VariantId = variantId,
                    NewStock = newStock,
                    Reason = reason,
                    OriginalPrice = originalPrice,
                    Price = currentPrice,
                    Timestamp = DateTime.UtcNow,
                    Message = reason == "order_placed" ?
                        $"🛒 Có người vừa đặt mua! Còn {newStock} sản phẩm" :
                        $"📦 Stock cập nhật: {newStock} sản phẩm"
                };

                await Clients.Group($"livestream_viewers_{livestreamId}")
                    .SendAsync("StockChanged", notification);

                _logger.LogInformation("✅ Stock change broadcasted: Product {ProductId} in livestream {LivestreamId}, new stock: {NewStock}, reason: {Reason}",
                    productId, livestreamId, newStock, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting stock change");
            }
        }
        /// <summary>
        /// Real-time cập nhật sản phẩm livestream bằng ID
        /// </summary>
        /// <param name="id">ID của livestream product</param>
        /// <param name="price">Giá mới</param>
        /// <param name="stock">Stock mới</param>
        /// <param name="isPin">Trạng thái pin</param>
        public async Task UpdateLivestreamProductById(string id, decimal price, int stock, bool isPin)
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

                // Execute update command
                var command = new UpdateLivestreamProductByIdCommand
                {
                    Id = Guid.Parse(id),
                    Price = price,
                    Stock = stock,
                    IsPin = isPin,
                    SellerId = userGuid
                };

                var result = await _mediator.Send(command);

                if (result != null)
                {
                    // ✅ BROADCAST real-time update to all viewers
                    await Clients.Group($"livestream_viewers_{result.LivestreamId}")
                        .SendAsync("LivestreamProductUpdated", new
                        {
                            Id = id,
                            LivestreamId = result.LivestreamId,
                            ProductId = result.ProductId,
                            VariantId = result.VariantId,
                            Price = price,
                            Stock = stock,
                            IsPin = isPin,
                            ProductName = result.ProductName,
                            UpdatedBy = userId,
                            Timestamp = DateTime.UtcNow,
                            Message = $"🔄 {result.ProductName} đã được cập nhật!"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = "LivestreamProductUpdated",
                        Id = id,
                        Product = result,
                        Message = "Cập nhật sản phẩm livestream thành công"
                    });

                    _logger.LogInformation("✅ Real-time livestream product update: Product ID {Id} updated",
                        id);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to update livestream product");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating livestream product real-time");
                await Clients.Caller.SendAsync("Error", $"Update livestream product failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Real-time pin/unpin sản phẩm livestream bằng ID
        /// </summary>
        /// <param name="id">ID của livestream product</param>
        /// <param name="isPin">true để pin, false để unpin</param>
        public async Task PinLivestreamProductById(string id, bool isPin)
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

                // Execute pin command
                var command = new PinProductByIdCommand
                {
                    Id = Guid.Parse(id),
                    IsPin = isPin,
                    SellerId = userGuid
                };

                var result = await _mediator.Send(command);

                if (result != null)
                {
                    // ✅ BROADCAST real-time pin/unpin to all viewers
                    await Clients.Group($"livestream_viewers_{result.LivestreamId}")
                        .SendAsync("LivestreamProductPinStatusChanged", new
                        {
                            Id = id,
                            LivestreamId = result.LivestreamId,
                            ProductId = result.ProductId,
                            VariantId = result.VariantId,
                            IsPin = isPin,
                            OriginalPrice = result.OriginalPrice,
                            Price = result.Price,
                            Stock = result.Stock,
                            ProductName = result.ProductName,
                            UpdatedBy = userId,
                            Timestamp = DateTime.UtcNow,
                            Message = isPin ?
                                $"📌 {result.ProductName} đã được ghim!" :
                                $"📌 {result.ProductName} đã bỏ ghim!"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = isPin ? "LivestreamProductPinned" : "LivestreamProductUnpinned",
                        Id = id,
                        Product = result,
                        Message = isPin ? "Sản phẩm đã được ghim" : "Sản phẩm đã bỏ ghim"
                    });

                    _logger.LogInformation("✅ Real-time livestream product pin update: Product ID {Id} {Action}",
                        id, isPin ? "pinned" : "unpinned");
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to update pin status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating livestream product pin status real-time");
                await Clients.Caller.SendAsync("Error", $"Pin update failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Real-time cập nhật stock sản phẩm livestream bằng ID
        /// </summary>
        /// <param name="id">ID của livestream product</param>
        /// <param name="newStock">Số lượng stock mới</param>
        public async Task UpdateLivestreamProductStockById(string id, int newStock)
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

                // Execute stock update command
                var command = new UpdateStockByIdCommand
                {
                    Id = Guid.Parse(id),
                    Stock = newStock,
                    SellerId = userGuid
                };

                var result = await _mediator.Send(command);

                if (result != null)
                {
                    // ✅ BROADCAST real-time stock update to all viewers
                    await Clients.Group($"livestream_viewers_{result.LivestreamId}")
                        .SendAsync("LivestreamProductStockUpdated", new
                        {
                            Id = id,
                            LivestreamId = result.LivestreamId,
                            ProductId = result.ProductId,
                            VariantId = result.VariantId,
                            NewStock = newStock,
                            OriginalPrice = result.OriginalPrice,
                            Price = result.Price,
                            ProductName = result.ProductName,
                            UpdatedBy = userId,
                            Timestamp = DateTime.UtcNow,
                            Message = $"📦 {result.ProductName} - Stock được cập nhật: {newStock} sản phẩm còn lại"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = "LivestreamProductStockUpdate",
                        Id = id,
                        Product = result,
                        NewStock = newStock,
                        Message = "Cập nhật stock sản phẩm thành công"
                    });

                    _logger.LogInformation("✅ Real-time livestream product stock update: Product ID {Id} updated to {NewStock}",
                        id, newStock);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to update product stock");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating livestream product stock real-time");
                await Clients.Caller.SendAsync("Error", $"Stock update failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Real-time xóa sản phẩm khỏi livestream bằng ID
        /// </summary>
        /// <param name="id">ID của livestream product</param>
        public async Task DeleteLivestreamProductById(string id)
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

                // Execute delete command
                var command = new DeleteLivestreamProductByIdCommand
                {
                    Id = Guid.Parse(id),
                    SellerId = userGuid
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    // ✅ BROADCAST real-time product removal to all viewers
                    await Clients.Group($"livestream_viewers_*")
                        .SendAsync("LivestreamProductDeleted", new
                        {
                            Id = id,
                            DeletedBy = userId,
                            Timestamp = DateTime.UtcNow,
                            Message = "🗑️ Một sản phẩm đã được gỡ khỏi livestream"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = "LivestreamProductDeleted",
                        Id = id,
                        Message = "Sản phẩm đã được xóa khỏi livestream"
                    });

                    _logger.LogInformation("✅ Real-time livestream product delete: Product ID {Id} removed",
                        id);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to delete livestream product");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting livestream product real-time");
                await Clients.Caller.SendAsync("Error", $"Delete product failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Real-time xóa sản phẩm khỏi livestream bằng ID (soft delete)
        /// </summary>
        /// <param name="id">ID của livestream product</param>
        /// <param name="reason">Lý do xóa</param>
        public async Task SoftDeleteLivestreamProductById(string id, string reason = "Removed by seller")
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

                // Execute soft delete command
                var command = new SoftDeleteLivestreamProductByIdCommand
                {
                    Id = Guid.Parse(id),
                    Reason = reason,
                    SellerId = userGuid
                };

                var result = await _mediator.Send(command);

                if (result)
                {
                    // ✅ BROADCAST real-time product soft removal to all viewers
                    await Clients.Group($"livestream_viewers_*")
                        .SendAsync("LivestreamProductSoftDeleted", new
                        {
                            Id = id,
                            Reason = reason,
                            DeletedBy = userId,
                            Timestamp = DateTime.UtcNow,
                            Message = $"🗑️ Sản phẩm đã được tạm thời gỡ bỏ: {reason}"
                        });

                    await Clients.Caller.SendAsync("UpdateSuccess", new
                    {
                        Action = "LivestreamProductSoftDeleted",
                        Id = id,
                        Reason = reason,
                        Message = "Sản phẩm đã được tạm thời gỡ bỏ khỏi livestream"
                    });

                    _logger.LogInformation("✅ Real-time livestream product soft delete: Product ID {Id} soft removed for reason: {Reason}",
                        id, reason);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to soft delete livestream product");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting livestream product real-time");
                await Clients.Caller.SendAsync("Error", $"Soft delete product failed: {ex.Message}");
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
