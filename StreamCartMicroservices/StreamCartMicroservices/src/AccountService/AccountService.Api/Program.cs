using dotenv.net;
using AccountService.Infrastructure.Extensions;
using Shared.Messaging.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Read .env file
DotEnv.Load();
var envConfig = Environment.GetEnvironmentVariables();

// Add services to the container
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);

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
app.UseAuthorization();
app.MapControllers();

app.Run();
