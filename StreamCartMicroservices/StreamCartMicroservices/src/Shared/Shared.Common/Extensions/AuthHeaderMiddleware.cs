using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Extensions
{
    public class AuthHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra header Authorization
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authHeaderValue = authHeader.ToString();

                // Nếu có token nhưng không bắt đầu bằng "Bearer "
                if (!string.IsNullOrEmpty(authHeaderValue) &&
                    !authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    // Thêm "Bearer " vào đầu token
                    context.Request.Headers.Remove("Authorization");
                    context.Request.Headers.Add("Authorization", $"Bearer {authHeaderValue}");
                }
            }

            await _next(context);
        }
    }
    public static class AuthHeaderMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthHeaderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthHeaderMiddleware>();
        }
    }
}
