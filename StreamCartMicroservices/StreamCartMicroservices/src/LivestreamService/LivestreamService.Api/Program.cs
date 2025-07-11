using LivestreamService.Api.Services;
using LivestreamService.Application.Commands;
using LivestreamService.Application.Extensions;
using LivestreamService.Application.Interfaces;
using LivestreamService.Infrastructure.Data;
using LivestreamService.Infrastructure.Extensions;
using LivestreamService.Infrastructure.Repositories;
using LivestreamService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL; // Add this import for PostgreSQL
using Shared.Common.Extensions;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Process configuration to replace ${ENV_VAR} placeholders
ReplaceConfigurationPlaceholders(builder.Configuration);

// Register MediatR and its handlers
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(CreateLivestreamCommand).Assembly);
});

// Add database - Changed to use PostgreSQL instead of SQL Server
builder.Services.AddDbContext<LivestreamDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHostedService<DatabaseInitializer>();

// Register services
builder.Services.AddScoped<ILivestreamRepository, LivestreamRepository>();

// Register HttpClients for service communication
builder.Services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
{
    var serviceUrl = builder.Configuration["ServiceUrls:ShopService"] ?? "http://shop-service";
    client.BaseAddress = new Uri(serviceUrl);
});

builder.Services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
{
    var serviceUrl = builder.Configuration["ServiceUrls:AccountService"] ?? "http://account-service";
    client.BaseAddress = new Uri(serviceUrl);
});

builder.Services.AddHttpClientFactory();
builder.Services.AddScoped<ILivekitService, LivekitService>();
builder.Services.AddScoped<IShopServiceClient, ShopServiceClient>();
builder.Services.AddScoped<IAccountServiceClient, AccountServiceClient>();

// Add shared settings configuration
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
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

app.UseConfiguredCors();
app.UseRouting();
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