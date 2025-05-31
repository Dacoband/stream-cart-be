using AccountService.Application.DTOs;
using AccountService.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Commands
{
    public class CreateAccountCommand : IRequest<AccountDto>
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Fullname { get; set; }

        [StringLength(255)]
        public string? AvatarURL { get; set; }

        public RoleType Role { get; set; } = RoleType.Customer;
    }
}
