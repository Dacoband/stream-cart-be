using ChatBoxService.Application.Interfaces;
using ChatBoxService.Infrastructure.Services;
using dotenv.net;
using Microsoft.OpenApi.Models;
using Shared.Common.Extensions;
using System.Text.RegularExpressions;

// Existing code
var builder = WebApplication.CreateBuilder(args);
DotEnv.Load();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ✅ THÊM DÒNG NÀY - Thay thế placeholders với environment variables
ReplaceConfigurationPlaceholders(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();

// Configure HttpClients for external services
builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    var serviceUrl = builder.Configuration["ServiceUrls:ProductService"];
    if (!string.IsNullOrEmpty(serviceUrl))
    {
        client.BaseAddress = new Uri(serviceUrl);
    }
});

builder.Services.AddHttpClient<IShopServiceClient, ShopServiceClient>(client =>
{
    var serviceUrl = builder.Configuration["ServiceUrls:ShopService"];
    if (!string.IsNullOrEmpty(serviceUrl))
    {
        client.BaseAddress = new Uri(serviceUrl);
    }
});

// Register services
builder.Services.AddScoped<IGeminiChatbotService, GeminiChatbotService>();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("*")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add shared settings configuration
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCurrentUserService();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ChatBot Service API",
        Version = "v1",
        Description = "API endpoints for AI chatbot functionality"
    });

    // Cấu hình JWT Authentication cho Swagger
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatBot Service API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(0);
});

// Bỏ hoặc điều kiện hóa HTTPS Redirection nếu sử dụng proxy
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.UseConfiguredCors();
app.UseRouting();
app.UseAuthHeaderMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ✅ THÊM HÀM NÀY
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