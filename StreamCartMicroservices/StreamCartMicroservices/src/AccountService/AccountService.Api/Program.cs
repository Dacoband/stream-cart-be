using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Extensions;
using AccountService.Infrastructure.Messaging.Consumers;
using dotenv.net;
using MassTransit;
using Shared.Common.Extensions;
using Shared.Messaging.Extensions;
using System.Text.RegularExpressions;

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

// Add services to the container
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration, x => {
    // Đăng ký consumers ở đây
    x.AddConsumer<AccountRegisteredConsumer>();

    // Nếu có nhiều consumer trong cùng một namespace
    var consumerAssembly = typeof(AccountRegisteredConsumer).Assembly;
    x.AddConsumers(consumerAssembly);
});

// Add shared settings configuration
builder.Services.AddAppSettings(builder.Configuration);

// Add CORS from shared library
builder.Services.AddConfiguredCors(builder.Configuration);

// Add JWT Authentication from shared library
builder.Services.AddJwtAuthentication(builder.Configuration);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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