using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.Services
{
    public class LivekitService : ILivekitService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LivekitService> _logger;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _livekitUrl;

        public LivekitService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<LivekitService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _apiKey = _configuration["LiveKit:ApiKey"];
            _apiSecret = _configuration["LiveKit:ApiSecret"];
            _livekitUrl = _configuration["LiveKit:Url"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiSecret) || string.IsNullOrEmpty(_livekitUrl))
            {
                throw new InvalidOperationException("LiveKit configuration is missing or incomplete");
            }
        }

        public async Task<string> CreateRoomAsync(string roomName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{_livekitUrl}/v1/rooms/{roomName}";

                var payload = new
                {
                    name = roomName,
                    empty_timeout = 300, // 5 minutes
                    max_participants = 100
                };

                var token = GenerateAccessToken(url, payload);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.PostAsync(url, new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"));

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadFromJsonAsync<JsonElement>();
                _logger.LogInformation("Room {RoomName} created in LiveKit", roomName);

                return roomName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room in LiveKit: {RoomName}", roomName);
                throw;
            }
        }

        public async Task<string> GenerateJoinTokenAsync(string roomName, string identity, bool canPublish)
        {
            try
            {
                var claims = new
                {
                    room = roomName,
                    sub = identity,
                    exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
                    iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    nbf = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    video = new
                    {
                        room = roomName,
                        roomJoin = true,
                        canPublish = canPublish,
                        canSubscribe = true
                    }
                };

                var token = GenerateJwt(claims);
                _logger.LogInformation("Generated token for {Identity} to join room {RoomName}", identity, roomName);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating join token for room {RoomName} and identity {Identity}", roomName, identity);
                throw;
            }
        }

        public async Task<bool> DeleteRoomAsync(string roomName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{_livekitUrl}/v1/rooms/{roomName}";

                var token = GenerateAccessToken(url, null, "DELETE");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.DeleteAsync(url);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Room {RoomName} deleted from LiveKit", roomName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room from LiveKit: {RoomName}", roomName);
                return false;
            }
        }

        public async Task<int> GetParticipantCountAsync(string roomName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{_livekitUrl}/v1/rooms/{roomName}/participants";

                var token = GenerateAccessToken(url, null, "GET");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var participantsData = await response.Content.ReadFromJsonAsync<ParticipantsResponse>();
                return participantsData?.Participants?.Count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting participants for room {RoomName}", roomName);
                return 0;
            }
        }

        private string GenerateAccessToken(string url, object payload = null, string method = "POST")
        {
            var claims = new
            {
                access = new
                {
                    method = method,
                    path = new Uri(url).PathAndQuery
                },
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                nbf = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            return GenerateJwt(claims);
        }

        private string GenerateJwt(object claims)
        {
            var header = new
            {
                alg = "HS256",
                typ = "JWT"
            };

            var headerJson = JsonSerializer.Serialize(header);
            var claimsJson = JsonSerializer.Serialize(claims);

            var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
            var claimsBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(claimsJson));

            var signature = ComputeSignature($"{headerBase64}.{claimsBase64}");

            return $"{headerBase64}.{claimsBase64}.{signature}";
        }

        private string ComputeSignature(string input)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Base64UrlEncode(hash);
        }

        private string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.TrimEnd('=');
            output = output.Replace('+', '-');
            output = output.Replace('/', '_');
            return output;
        }
    }

    internal class ParticipantsResponse
    {
        public List<ParticipantInfo> Participants { get; set; } = new List<ParticipantInfo>();
    }

    internal class ParticipantInfo
    {
        public string Sid { get; set; }
        public string Identity { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public long JoinedAt { get; set; }
    }
}