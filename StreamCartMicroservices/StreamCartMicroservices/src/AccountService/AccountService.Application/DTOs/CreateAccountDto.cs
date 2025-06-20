using AccountService.Application.Validators;
using AccountService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs
{
    public class CreateAccountDto
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [ValidateVietnamesePhoneNumber(ErrorMessage = "Invalid Vietnamese phone number. Must be 10-12 digits starting with 0.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fullname is required")]
        [MinLength(2, ErrorMessage = "Fullname must be at least 2 characters long")]
        public string Fullname { get; set; } = string.Empty;
        public string? AvatarURL { get; set; }
        [Required(ErrorMessage = "Role is required")]
        [RoleValidation(ErrorMessage = "Role must be either Customer or Seller")]
        public RoleType Role { get; set; } = RoleType.Customer;
    }
}
