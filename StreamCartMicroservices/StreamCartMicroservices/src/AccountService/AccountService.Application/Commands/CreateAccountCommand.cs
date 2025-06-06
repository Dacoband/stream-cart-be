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
        
        // này sẽ được set trong AccountManagementService, không phơi ra ngoài
        public RoleType Role { get; set; } = RoleType.Customer;
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
    }
}
