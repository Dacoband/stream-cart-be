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

            // ✅ FIX: Better logging for debugging
            _logger.LogInformation("LiveKit Configuration - ApiKey: {ApiKey}, Url: {Url}",
                string.IsNullOrEmpty(_apiKey) ? "NOT SET" : $"{_apiKey.Substring(0, Math.Min(4, _apiKey.Length))}...",
                _livekitUrl ?? "NOT SET");

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiSecret) || string.IsNullOrEmpty(_livekitUrl))
            {
                var missingConfigs = new List<string>();
                if (string.IsNullOrEmpty(_apiKey)) missingConfigs.Add("ApiKey");
                if (string.IsNullOrEmpty(_apiSecret)) missingConfigs.Add("ApiSecret");
                if (string.IsNullOrEmpty(_livekitUrl)) missingConfigs.Add("Url");

                var errorMsg = $"LiveKit configuration is missing: {string.Join(", ", missingConfigs)}";
                _logger.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }
        }

        public async Task<string> CreateRoomAsync(string roomName)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                // Use the Twirp protocol endpoint format - this is what LiveKit expects
                var url = $"{_livekitUrl}/twirp/livekit.RoomService/CreateRoom";
                _logger.LogInformation("Creating room using endpoint: {Url}", url);

                var payload = new
                {
                    name = roomName,
                    empty_timeout = 300,
                    max_participants = 100
                };

                // Generate JWT token for authentication
                var token = GenerateAccessTokenForRoomCreate(roomName);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Make the API call
                var response = await client.PostAsync(url, new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Room creation failed with status: {StatusCode}, Response: {Response}",
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    throw new HttpRequestException($"LiveKit room creation failed with status: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Room {RoomName} created in LiveKit. Response: {Response}",
                    roomName, responseContent);

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
                    iss = _apiKey,
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
        private string GenerateAccessTokenForRoomCreate(string roomName)
        {
            var claims = new
            {
                iss = _apiKey,
                sub = "server",
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
                nbf = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                video = new
                {
                    roomCreate = true,
                    roomAdmin = true,
                    room = roomName
                }
            };

            return GenerateJwt(claims);
        }

        private string GenerateAccessToken(string url, object payload = null, string method = "POST")
        {
            var claims = new
            {
                iss = _apiKey,
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
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var headerJson = JsonSerializer.Serialize(header,options);
            var claimsJson = JsonSerializer.Serialize(claims,options);

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
        //Phần chat realtime 
        public async Task<string> CreateChatRoomAsync(Guid shopId, Guid customerId)
        {
            var chatRoomName = $"chat-shop-{shopId}-customer-{customerId}";

            try
            {
                await CreateRoomAsync(chatRoomName);
                _logger.LogInformation("Created chat room: {ChatRoomName}", chatRoomName);
                return chatRoomName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat room for shop {ShopId} and customer {CustomerId}", shopId, customerId);
                throw;
            }
        }
        public async Task<string> GenerateChatTokenAsync(string roomName, string participantName, bool isShop = false)
        {
            try
            {
                var claims = new
                {
                    iss = _apiKey,
                    room = roomName,
                    sub = participantName,
                    exp = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds(),
                    iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    nbf = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    video = new
                    {
                        room = roomName,
                        roomJoin = true,
                        canPublish = false,  // Không cần publish video/audio cho chat
                        canSubscribe = false, // Không cần subscribe video/audio
                        canPublishData = true,  // Cho phép gửi data (chat messages)
                        canUpdateOwnMetadata = true,
                        hidden = false
                    }
                };

                var token = GenerateJwt(claims);
                _logger.LogInformation("Generated chat token for {ParticipantName} in room {RoomName}", participantName, roomName);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating chat token for room {RoomName} and participant {ParticipantName}", roomName, participantName);
                throw;
            }
        }
        public async Task<bool> SendDataMessageAsync(string roomName, string senderId, object message)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"{_livekitUrl}/twirp/livekit.RoomService/SendData";

                var payload = new
                {
                    room = roomName,
                    data = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message))),
                    kind = "reliable", // hoặc "lossy" 
                    destination_sids = new string[] { } // Gửi cho tất cả participants
                };

                var token = GenerateAccessTokenForRoomCreate(roomName);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(url, new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Data message sent successfully to room {RoomName}", roomName);
                    return true;
                }
                else
                {
                    _logger.LogError("Failed to send data message to room {RoomName}: {StatusCode}", roomName, response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending data message to room {RoomName}", roomName);
                return false;
            }
        }
        public async Task<bool> IsRoomActiveAsync(string roomName)
        {
            try
            {
                var participantCount = await GetParticipantCountAsync(roomName);
                return participantCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if room {RoomName} is active", roomName);
                return false;
            }
        }



    }

    internal class ParticipantsResponse
    {
        public List<ParticipantInfo> Participants { get; set; } = new List<ParticipantInfo>();
    }

    internal class ParticipantInfo
    {
        public string? Sid { get; set; }
        public string? Identity { get; set; }
        public string? State { get; set; }
        public string? Name { get; set; }
        public long JoinedAt { get; set; }
    }
}