using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Api.Middleware
{
    public class SignalRDebugMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SignalRDebugMiddleware> _logger;

        public SignalRDebugMiddleware(RequestDelegate next, ILogger<SignalRDebugMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only log SignalR related requests
            if (context.Request.Path.StartsWithSegments("/signalrchat") ||
                context.Request.Path.StartsWithSegments("/testhub") ||
                context.Request.Path.StartsWithSegments("/notificationHub"))
            {
                _logger.LogInformation("🔧 SignalR Request: {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                _logger.LogInformation("🔧 Headers: {Headers}",
                    string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}")));
                _logger.LogInformation("🔧 Query: {Query}",
                    string.Join(", ", context.Request.Query.Select(q => $"{q.Key}={q.Value}")));
                _logger.LogInformation("🔧 Protocol: {Protocol}", context.Request.Protocol);
                _logger.LogInformation("🔧 Scheme: {Scheme}", context.Request.Scheme);
                _logger.LogInformation("🔧 Host: {Host}", context.Request.Host);
                _logger.LogInformation("🔧 User Agent: {UserAgent}",
                    context.Request.Headers["User-Agent"].FirstOrDefault() ?? "None");

                // Log WebSocket specific info
                if (context.WebSockets.IsWebSocketRequest)
                {
                    _logger.LogInformation("🔧 WebSocket Request Detected");
                    _logger.LogInformation("🔧 WebSocket SubProtocols: {SubProtocols}",
                        string.Join(", ", context.WebSockets.WebSocketRequestedProtocols));
                }

                // Capture response
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🔧 Exception in SignalR request: {Path}", context.Request.Path);
                    throw;
                }
                finally
                {
                    _logger.LogInformation("🔧 Response Status: {StatusCode}", context.Response.StatusCode);
                    _logger.LogInformation("🔧 Response Headers: {Headers}",
                        string.Join(", ", context.Response.Headers.Select(h => $"{h.Key}={h.Value}")));

                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                    context.Response.Body = originalBodyStream;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}