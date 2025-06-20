using AccountService.Application.DTOs;
using AccountService.Domain.Enums;
using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class CreateAccountCommand : IRequest<AccountDto>
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Fullname { get; set; }
        public string? AvatarURL { get; set; }
        public RoleType Role { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public decimal CompleteRate { get; set; } = 1.0m;
    }
}
