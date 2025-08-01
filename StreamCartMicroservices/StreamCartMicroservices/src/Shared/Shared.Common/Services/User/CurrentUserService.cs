using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;

namespace Shared.Common.Services.User
{
    public interface ICurrentUserService
    {
        Guid GetUserId();
        string GetUsername();
        string[] GetRoles();
        bool IsInRole(string role);
        bool IsAuthenticated();
        string GetUserEmail();
        string GetShopId();
        string GetAccessToken();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Guid GetUserId()
        {
            try
            {
                // Logging để debug
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    Console.WriteLine("HttpContext is null");
                    throw new UnauthorizedAccessException("HttpContext is null");
                }

                var user = httpContext.User;
                if (user == null)
                {
                    Console.WriteLine("User is null");
                    throw new UnauthorizedAccessException("User is null");
                }

                if (!user.Identity.IsAuthenticated)
                {
                    Console.WriteLine("User is not authenticated");
                    throw new UnauthorizedAccessException("User is not authenticated");
                }

                // In ra tất cả các claims để debug
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                                  ?? user.FindFirst("id")
                                  ?? user.FindFirst("sub");

                if (userIdClaim == null)
                {
                    Console.WriteLine("UserId claim not found");
                    return Guid.Empty;
                }

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    Console.WriteLine($"Failed to parse {userIdClaim.Value} as GUID");
                    return Guid.Empty;
                }

                return userId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user ID: {ex.Message}");
                return Guid.Empty;
            }
        }

        public string GetUsername()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value
                   ?? _httpContextAccessor.HttpContext?.User.FindFirst("unique_name")?.Value
                   ?? throw new UnauthorizedAccessException("Username not found in token");
        }

        public string[] GetRoles()
        {
            // Nếu role là 1 hoặc nhiều, đều xử lý được
            var roleClaims = _httpContextAccessor.HttpContext?.User.Claims
                              .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                              .Select(c => c.Value)
                              .ToArray();

            return roleClaims ?? Array.Empty<string>();
        }

        public bool IsInRole(string role)
        {
            return GetRoles().Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        }

        public string GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value
                   ?? _httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value
                   ?? throw new UnauthorizedAccessException("Email not found in token");
        }

        public string GetShopId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst("ShopId")?.Value
                   ?? string.Empty;
        }
        public string GetAccessToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new UnauthorizedAccessException("HttpContext is null");

            var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Authorization header missing or invalid");

            return authHeader.Substring("Bearer ".Length).Trim();
        }
    }
}