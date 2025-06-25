using Appwrite;
using dotenv.net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductService.Api.Services;
using ProductService.Application.Commands.AttributeCommands;
using ProductService.Application.Commands.AttributeValueCommands;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Application.Commands.ProductComands;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.DTOs.Combinations;
using ProductService.Application.DTOs.Variants;
using ProductService.Application.Extensions;
using ProductService.Application.Handlers.AttributeHandlers;
using ProductService.Application.Handlers.AttributeValueHandlers;
using ProductService.Application.Handlers.CombinationHandlers;
using ProductService.Application.Handlers.VariantHandlers;
using ProductService.Application.Queries.AttributeValueQueries;
using ProductService.Infrastructure.Extensions;
using Shared.Common.Extensions;
using Shared.Messaging.Extensions;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();


// Thêm cấu hình
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

ReplaceConfigurationPlaceholders(builder.Configuration);

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
    config.RegisterServicesFromAssembly(typeof(GetAllAttributeValuesQuery).Assembly);

});
//builder.Services.AddMediator(cfg =>
//{
//    cfg.AddConsumersFromNamespaceContaining<UploadImageHandler>();
//});
// Add services to the container
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddHostedService<DatabaseInitializer>();

// Thêm các dịch vụ chung từ Shared
builder.Services.AddAppSettings(builder.Configuration);
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
// Add this line to Program.cs services registration
builder.Services.AddAppwriteServices(builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product Service API",
        Version = "v1",
        Description = "API endpoints for product management"
    });
    c.CustomSchemaIds(type => {
        // Use fully qualified type name to avoid conflicts
        return type.FullName;
    });

    // Cấu hình JWT Authentication cho Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

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