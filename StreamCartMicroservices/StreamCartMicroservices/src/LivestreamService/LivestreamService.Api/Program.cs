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
using Npgsql.EntityFrameworkCore.PostgreSQL; // Add this import for PostgreSQL
using Shared.Common.Extensions;
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
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
//builder.Services.AddMessaging(builder.Configuration);
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
// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100 KB
});

// Register SignalR chat service
builder.Services.AddScoped<ISignalRChatService, SignalRChatService>();

// Add JWT authentication for SignalR
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/signalrchat") || path.StartsWithSegments("/notificationHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Livestream Service API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(0);
});

// Conditionally apply HTTPS redirection
if (!builder.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.MapHub<SignalRChatHub>("/signalrchat");
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chatHub");
app.UseRouting();
app.UseHttpsRedirection();
app.UseConfiguredCors();
app.UseAuthHeaderMiddleware();
app.UseAuthentication();
app.UseAuthorization();
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