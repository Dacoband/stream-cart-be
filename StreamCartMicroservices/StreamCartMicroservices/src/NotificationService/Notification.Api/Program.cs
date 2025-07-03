using dotenv.net;
using MassTransit;
using Microsoft.OpenApi.Models;
using Notification.Api.Hubs;
using Notification.Application.Command;
using Notification.Application.Consumer;
using Notification.Application.Extensions;
using Notification.Application.Interfaces;
using Notification.Application.Queries;
using Notification.Infrastrcture.Extention;
using Shared.Common.Extensions;
using Shared.Messaging.Extensions;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Load .env
DotEnv.Load();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();
ReplaceConfigurationPlaceholders(builder.Configuration);

// Add core services
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();

// App services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT, Email, Appwrite, etc.
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddAppwriteServices(builder.Configuration);
builder.Services.AddCurrentUserService();

// SignalR + RealTimeNotifier
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealTimeNotifier, RealtimeNotifier>();

// MediatR
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(GetMyNotificationQuery).Assembly);
    config.RegisterServicesFromAssembly(typeof(MarkAsRead).Assembly);
});

// MassTransit (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FlashSaleConsumer>();
    x.AddConsumer<OrderChangeComsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQSettings:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQSettings:Username"]);
            h.Password(builder.Configuration["RabbitMQSettings:Password"]);
        });

        cfg.ReceiveEndpoint("flash-sale-notification-queue", e =>
        {
            e.ConfigureConsumer<FlashSaleConsumer>(context);
        });

        cfg.ReceiveEndpoint("order-changed-notification-queue", e =>
        {
            e.ConfigureConsumer<OrderChangeComsumer>(context);
        });
    });
});

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "API endpoints for Notification"
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

var app = builder.Build();

// Middleware pipeline
if (!builder.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Service API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(0);
});

app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseConfiguredCors();
app.UseAuthHeaderMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();


// Helper method for replacing ${ENV_VAR} placeholders
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
