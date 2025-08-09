using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Hubs
{
    /// <summary>
    /// Minimal hub for testing handshake issues - NO AUTHENTICATION
    /// </summary>
    public class TestHub : Hub
    {
        private readonly ILogger<TestHub> _logger;

        public TestHub(ILogger<TestHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("🔧 TestHub: Connection {ConnectionId} started", connectionId);

            try
            {
                await base.OnConnectedAsync();
                _logger.LogInformation("✅ TestHub: Handshake completed for {ConnectionId}", connectionId);

                // Send immediate confirmation
                await Clients.Caller.SendAsync("Connected", new
                {
                    ConnectionId = connectionId,
                    Message = "TestHub connected successfully!",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ TestHub: Handshake failed for {ConnectionId}", connectionId);
                throw; // Let SignalR handle the error
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("🔧 TestHub: Connection {ConnectionId} disconnected. Exception: {Exception}",
                connectionId, exception?.Message ?? "None");

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTestMessage(string message)
        {
            _logger.LogInformation("🔧 TestHub: Received test message: {Message}", message);

            await Clients.All.SendAsync("TestMessageReceived", new
            {
                OriginalMessage = message,
                Response = $"Echo: {message}",
                Timestamp = DateTime.UtcNow,
                From = Context.ConnectionId
            });
        }

        public async Task PingTest()
        {
            await Clients.Caller.SendAsync("PongTest", new
            {
                Message = "Pong from TestHub",
                Timestamp = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId
            });
        }
    }
}