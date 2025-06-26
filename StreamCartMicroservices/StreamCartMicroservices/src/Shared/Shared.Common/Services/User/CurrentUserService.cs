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
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? _httpContextAccessor.HttpContext?.User.FindFirst("nameid");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token or is not a valid GUID");
            }

            return userId;
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
    }
}
