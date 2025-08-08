using AutoMapper;
using dotenv.net;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Shared.Common.Extensions;
using Shared.Common.Settings;
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

// ✅ Configure CORS first, before other services
builder.Services.AddConfiguredCors(builder.Configuration);

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

// ✅ Enhanced SignalR configuration for HTTPS/Production
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100KB
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);

    // ✅ Additional configuration for production
    options.StreamBufferCapacity = 10;
});

// ✅ Enhanced JWT configuration for SignalR with HTTPS support
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
            Console.WriteLine($"[SignalR JWT] Scheme: {context.Request.Scheme}");
            Console.WriteLine($"[SignalR JWT] Host: {context.Request.Host}");

            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/signalrchat") || path.StartsWithSegments("/notificationHub")))
            {
                context.Token = accessToken;
                Console.WriteLine($"[SignalR JWT] Token set for SignalR connection");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[SignalR JWT] Authentication FAILED: {context.Exception.Message}");
            Console.WriteLine($"[SignalR JWT] Exception: {context.Exception}");
            Console.WriteLine($"[SignalR JWT] Path: {context.Request.Path}");
            Console.WriteLine($"[SignalR JWT] Scheme: {context.Request.Scheme}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"[SignalR JWT] Token VALIDATED successfully");
            Console.WriteLine($"[SignalR JWT] User: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"[SignalR JWT] Authentication Challenge");
            Console.WriteLine($"[SignalR JWT] Error: {context.Error}");
            Console.WriteLine($"[SignalR JWT] ErrorDescription: {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// ✅ CRITICAL: Use forwarded headers for proxy/load balancer
app.UseForwardedHeaders();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Livestream Service API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(0);
});

// ✅ CRITICAL: Correct middleware order for HTTPS and SignalR
app.UseRouting();

// ✅ CRITICAL: CORS must come before authentication for SignalR to work with HTTPS
app.UseCors("DefaultCorsPolicy");

// ✅ HTTPS redirection - only for direct HTTPS, not behind proxy
if (!builder.Environment.IsEnvironment("Docker") && !builder.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// ✅ Custom middleware for auth headers
app.UseAuthHeaderMiddleware();

// ✅ Authentication and Authorization - correct order
app.UseAuthentication();
app.UseAuthorization();

// ✅ CRITICAL: Map SignalR hubs with proper configuration for HTTPS
app.MapHub<SignalRChatHub>("/signalrchat", options =>
{
    // ✅ FIXED: Use combination of transport types instead of .All
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;

    // ✅ Allow all connection types for better compatibility
    options.ApplicationMaxBufferSize = 64 * 1024; // 64KB
    options.TransportMaxBufferSize = 64 * 1024; // 64KB

    // ✅ WebSocket specific settings
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
});

app.MapHub<NotificationHub>("/notificationHub", options =>
{
    // ✅ FIXED: Use combination of transport types instead of .All
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents |
                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;

    options.ApplicationMaxBufferSize = 64 * 1024;
    options.TransportMaxBufferSize = 64 * 1024;
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(90);
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