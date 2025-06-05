using AccountService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs
{
    public class CreateAccountDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Fullname { get; set; }
        public string? AvatarURL { get; set; }
        public RoleType Role { get; set; } = RoleType.Customer; 
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
    }
}
