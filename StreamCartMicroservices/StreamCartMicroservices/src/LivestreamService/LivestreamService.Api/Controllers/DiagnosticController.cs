using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LivestreamService.Api.Controllers
{
    [ApiController]
    [Route("api/diagnostics")]
    public class DiagnosticController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DiagnosticController> _logger;
        private readonly IConfiguration _configuration;

        public DiagnosticController(
            IHttpClientFactory httpClientFactory,
            ILogger<DiagnosticController> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("livekit-test")]
        [AllowAnonymous]
        public async Task<IActionResult> TestLiveKitConnection()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var livekitUrl = _configuration["LiveKit:Url"] ?? "http://livekit:7880";

                // Test basic connection to LiveKit
                var baseResponse = await client.GetAsync(livekitUrl);

                // Log configuration values
                _logger.LogInformation("LiveKit URL: {Url}", livekitUrl);
                _logger.LogInformation("API Key available: {HasKey}", !string.IsNullOrEmpty(_configuration["LiveKit:ApiKey"]));
                _logger.LogInformation("API Secret available: {HasSecret}", !string.IsNullOrEmpty(_configuration["LiveKit:ApiSecret"]));

                return Ok(new
                {
                    Config = new
                    {
                        Url = livekitUrl,
                        ApiKey = _configuration["LiveKit:ApiKey"],
                        HasApiSecret = !string.IsNullOrEmpty(_configuration["LiveKit:ApiSecret"])
                    },
                    BaseConnection = new
                    {
                        StatusCode = baseResponse.StatusCode,
                        Content = await baseResponse.Content.ReadAsStringAsync()
                    },
                    Message = "LiveKit server is running. API endpoints require Twirp protocol and proper authentication."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing LiveKit connection");
                return StatusCode(500, new { Error = ex.Message, Details = ex.ToString() });
            }
        }
    }
}