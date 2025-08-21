using AutoMapper;
using dotenv.net;
using LivestreamService.Api.Middleware;
using LivestreamService.Api.Services;
using LivestreamService.Application.Commands;
using LivestreamService.Application.Extensions;
using LivestreamService.Application.Interfaces;
using LivestreamService.Infrastructure.Data;
using LivestreamService.Infrastructure.Extensions;
using LivestreamService.Infrastructure.Hubs;
using LivestreamService.Infrastructure.Repositories;
using LivestreamService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Shared.Common.Extensions;
using Shared.Common.Settings;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();
// Load environment variables
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Process configuration to replace ${ENV_VAR} placeholders
ReplaceConfigurationPlaceholders(builder.Configuration);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHostedService<DatabaseInitializer>();

builder.Services.AddScoped<ILivestreamRepository, LivestreamRepository>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<ILivekitService, LivekitService>();

builder.Services.AddAppSettings(builder.Configuration);

// ✅ FIX: CORS configuration để support credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:8080", "http://127.0.0.1:3000", "http://127.0.0.1:8080", "file://", "https://brightpa.me", "null",
      "http://brightpa.me", "https://streamcart.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); 
    });

    // ✅ Additional policy cho testing từ file://
    options.AddPolicy("FileTestingPolicy", policy =>
    {
        policy.AllowAnyOrigin() // ✅ For file:// testing only
              .AllowAnyMethod()
              .AllowAnyHeader();
        // ✅ Note: Cannot use AllowCredentials with AllowAnyOrigin
    });

    // ✅ Development policy
    options.AddPolicy("DevCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:8080", "http://127.0.0.1:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
// ✅ Add WebSocket services
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromMinutes(2);
    options.ReceiveBufferSize = 4 * 1024;
});

// ✅ Configure forwarded headers for reverse proxy/load balancer
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                              Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAppwriteServices(builder.Configuration);
builder.Services.AddCurrentUserService();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Livestream Service API",
        Version = "v1",
        Description = "API endpoints for livestream management"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register SignalR chat service
builder.Services.AddScoped<ISignalRChatService, SignalRChatService>();

// ✅ Enhanced SignalR configuration for HTTPS/Production with detailed logging
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100KB
    options.HandshakeTimeout = TimeSpan.FromSeconds(90); // ✅ Increase handshake timeout
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(90); // ✅ Increase client timeout
    options.StreamBufferCapacity = 10;
});

// ✅ Enhanced JWT configuration for Docker
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            Console.WriteLine($"[SignalR JWT] Path: {path}");
            Console.WriteLine($"[SignalR JWT] Token present: {!string.IsNullOrEmpty(accessToken)}");

            // ✅ FIXED: Check for SignalR paths and set token
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/signalrchat") ||
                 path.StartsWithSegments("/notificationHub") ||
                 path.StartsWithSegments("/testhub"))) // ✅ Add testhub
            {
                context.Token = accessToken;
                Console.WriteLine($"[SignalR JWT] Token set for SignalR connection");
            }

            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            var path = context.Request.Path;

            // Nếu là SignalR → bỏ qua trả 401, để handshake qua
            if (path.StartsWithSegments("/signalrchat") ||
                path.StartsWithSegments("/notificationHub") ||
                path.StartsWithSegments("/testhub"))
            {
                context.HandleResponse();
                context.Response.StatusCode = 200;
                return;
            }

            // Các API thường → giữ behavior cũ
            context.HandleResponse();
            context.Response.ContentType = "application/json";

            string message = "Token is invalid";
            string errorCode = ApiCodes.TOKEN_INVALID;
            int statusCode = StatusCodes.Status401Unauthorized;

            if (context.AuthenticateFailure is SecurityTokenExpiredException)
            {
                message = "Token is expired";
                errorCode = ApiCodes.TOKEN_EXPIRED;
                statusCode = StatusCodes.Status403Forbidden;
            }
            else if (string.IsNullOrEmpty(context.Request.Headers["Authorization"]))
            {
                message = "No token provided";
                errorCode = ApiCodes.UNAUTHENTICATED;
            }

            context.Response.StatusCode = statusCode;
            var result = JsonSerializer.Serialize(new { errorCode, message });
            await context.Response.WriteAsync(result);
        }
    };
});
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Trace);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Trace);
var app = builder.Build();

// ✅ Add debug middleware first
app.UseMiddleware<SignalRDebugMiddleware>();

// ✅ CRITICAL: Use forwarded headers for proxy/load balancer
app.UseForwardedHeaders();
app.UseRouting();
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Livestream Service API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(0);
});



// ✅ CRITICAL: CORS must come before authentication for SignalR to work
app.UseCors("DefaultCorsPolicy");
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

// ✅ Map TestHub for debugging (NO AUTHORIZATION)
app.MapHub<TestHub>("/testhub", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
    options.ApplicationMaxBufferSize = 64 * 1024;
    options.TransportMaxBufferSize = 64 * 1024;
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
    options.AllowStatefulReconnects = false;
});

// ✅ Map other hubs
app.MapHub<SignalRChatHub>("/signalrchat", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
    options.ApplicationMaxBufferSize = 64 * 1024;
    options.TransportMaxBufferSize = 64 * 1024;
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
    options.AllowStatefulReconnects = false;
});

app.MapHub<NotificationHub>("/notificationHub", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
    options.ApplicationMaxBufferSize = 64 * 1024;
    options.TransportMaxBufferSize = 64 * 1024;
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
    options.AllowStatefulReconnects = false;
});

app.MapControllers();

app.Run();

// Helper method to replace ${ENV_VAR} placeholders in configuration
void ReplaceConfigurationPlaceholders(IConfigurationRoot config)
{
    var regex = new Regex(@"\${([^}]+)}");

    foreach (var provider in config.Providers.ToList())
    {
        if (provider is Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider jsonProvider)
        {
            var data = jsonProvider.GetType()
                .GetProperty("Data", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(jsonProvider) as IDictionary<string, string>;

            if (data != null)
            {
                foreach (var key in data.Keys.ToList())
                {
                    if (data[key] != null)
                    {
                        data[key] = regex.Replace(data[key], match =>
                        {
                            var envVarName = match.Groups[1].Value;
                            return Environment.GetEnvironmentVariable(envVarName) ?? string.Empty;
                        });
                    }
                }
            }
        }
    }
}