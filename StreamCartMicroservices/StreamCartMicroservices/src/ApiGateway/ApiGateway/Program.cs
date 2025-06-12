using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Shared.Common.Extensions;
using MMLib.SwaggerForOcelot.DependencyInjection;
using System.Text.Json.Serialization;
using ApiGateway.Middleware;
using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    DotEnv.Load(); // Đọc file .env nếu có
    Console.WriteLine("JWT Secret Key: " + Environment.GetEnvironmentVariable("JWT_SECRET_KEY"));
}
var env = builder.Environment.EnvironmentName;
Console.WriteLine($"Current environment: {env}");

string ocelotConfigFile = File.Exists($"ocelot.{env}.json")
    ? $"ocelot.{env}.json"
    : "ocelot.json";
Console.WriteLine($"Using Ocelot configuration: {ocelotConfigFile}");


// Add configuration sources
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true)  // Chỉ load một file Ocelot
    .AddEnvironmentVariables();


builder.Services.AddConfiguredCors(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddJwtAuthentication(builder.Configuration);

// Healthy check endpoint
builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StreamCart API Gateway",
        Version = "v1",
        Description = "API Gateway for StreamCart microservices"
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

// Add Ocelot and Swagger for Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddOcelot(builder.Configuration)
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    Console.WriteLine("Running in Development mode");
}
else
{
    Console.WriteLine($"Running in {app.Environment.EnvironmentName} mode");
}

app.UseHttpsRedirection();

app.UseConfiguredCors();
app.UseExceptionHandler("/error");
app.UseRouting();
app.UseAuthHeaderMiddleware();
app.UseAuthentication();
app.UseAuthorization();

// SwaggerUI for Ocelot
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.ReConfigureUpstreamSwaggerJson = AlterUpstreamSwaggerJson;
});

app.MapHealthChecks("/health");
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.UseOcelot().Wait();
app.Run();

//Custom method to alter upstream Swagger JSON
string AlterUpstreamSwaggerJson(HttpContext context, string swaggerJson)
{
    var swagger =  System.Text.Json.JsonDocument.Parse(swaggerJson);
    var root = swagger.RootElement;

    using var jsonDoc = System.Text.Json.JsonDocument.Parse(swaggerJson);
    var output = new System.Text.Json.Nodes.JsonObject();
    foreach(var property in root.EnumerateObject())
    {
        if (property.Name == "info")
        {
            var infoObject = new System.Text.Json.Nodes.JsonObject
            {
                ["title"] = "StreamCart API Gateway",
                ["version"] = "v1",
                ["description"] = "API Gateway for StreamCart microservices"
            };
            output["info"] = infoObject;
        }
        else
        {
            output[property.Name] = System.Text.Json.Nodes.JsonNode.Parse(property.Value.GetRawText());
        }
    }
    return output.ToJsonString(new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    });
}