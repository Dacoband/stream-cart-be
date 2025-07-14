using System;

namespace LivestreamService.Application.DTOs
{
    public class AccountDTO
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }

        public string? Fullname { get; set; }
        public int? Role { get; set; }
        public bool IsVerified { get; set; }

    }
}
