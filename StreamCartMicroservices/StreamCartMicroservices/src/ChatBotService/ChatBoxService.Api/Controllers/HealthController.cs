using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace ChatBoxService.Api.Controllers
{
    [ApiController]
    [Route("api/checkredis")]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            IConnectionMultiplexer redis,
            IConfiguration configuration,
            ILogger<HealthController> logger)
        {
            _redis = redis;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        [Route("/health")]
        public async Task<IActionResult> Health()
        {
            try
            {
                var healthStatus = new
                {
                    service = "ChatBot Service",
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    dependencies = await CheckDependencies()
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    service = "ChatBot Service",
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

        private async Task<object> CheckDependencies()
        {
            var dependencies = new Dictionary<string, object>();

            // Check Redis
            try
            {
                var database = _redis.GetDatabase();
                var pingResult = await database.PingAsync();
                dependencies["redis"] = new
                {
                    status = "healthy",
                    responseTime = $"{pingResult.TotalMilliseconds:F2}ms"
                };
            }
            catch (Exception ex)
            {
                dependencies["redis"] = new
                {
                    status = "unhealthy",
                    error = ex.Message
                };
            }

            // Check configuration
            try
            {
                var geminiKey = _configuration["GEMINI_API_KEY"] ?? _configuration["Gemini:ApiKey"];
                var productServiceUrl = _configuration["ServiceUrls:ProductService"];
                var shopServiceUrl = _configuration["ServiceUrls:ShopService"];

                dependencies["configuration"] = new
                {
                    status = "healthy",
                    geminiConfigured = !string.IsNullOrEmpty(geminiKey),
                    productServiceConfigured = !string.IsNullOrEmpty(productServiceUrl),
                    shopServiceConfigured = !string.IsNullOrEmpty(shopServiceUrl)
                };
            }
            catch (Exception ex)
            {
                dependencies["configuration"] = new
                {
                    status = "unhealthy",
                    error = ex.Message
                };
            }

            return dependencies;
        }
    }
}