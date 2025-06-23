using CartService.Api.Services;
using CartService.Infrastructure.Extensions;
using dotenv.net;
using MassTransit;
using Microsoft.OpenApi.Models;
using System.Text.RegularExpressions;
using Shared.Common.Extensions;
using Shared.Messaging.Extensions;
using System.Text.RegularExpressions;
using CartService.Application.Command;
using CartService.Application.Extensions;
using CartService.Application.Services;
using CartService.Application.Interfaces;
using System;
using Npgsql;
using CartService.Application.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Read .env file
DotEnv.Load();
var envConfig = Environment.GetEnvironmentVariables();

// Add custom configuration with environment variable substitution
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Process configuration to replace ${ENV_VAR} placeholders
ReplaceConfigurationPlaceholders(builder.Configuration);

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(AddToCartCommand).Assembly);
});

builder.Services.AddHttpClient<IProductService, ProductService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            // Accept invalid HTTPS certificate for dev only
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
builder.Services.AddHttpClient<IProductService, ProductService>(client =>
{
    client.BaseAddress = new Uri("http://product-service");
    client.Timeout = TimeSpan.FromSeconds(30); // Tăng timeout
    client.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10MB
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
Npgsql.NpgsqlConnection.GlobalTypeMapper
    .UseJsonNet();

// Add services to the container
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddApplicationServices();
builder.Services.AddMessaging(builder.Configuration, x =>
{
    // Đăng ký consumers ở đây
    x.AddConsumer<ProductUpdatedConsumer>();

    // Nếu có nhiều consumer trong cùng một namespace
    var consumerAssembly = typeof(ProductUpdatedConsumer).Assembly;
    x.AddConsumers(consumerAssembly);
});

// Add shared settings configuration
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddAppwriteServices(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Account Service API",
        Version = "v1",
        Description = "API endpoints for account management and authentication"
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

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Service API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(0);
});

// Bỏ hoặc điều kiện hóa HTTPS Redirection nếu bạn sử dụng proxy ở trước
if (!builder.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.UseConfiguredCors();
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
