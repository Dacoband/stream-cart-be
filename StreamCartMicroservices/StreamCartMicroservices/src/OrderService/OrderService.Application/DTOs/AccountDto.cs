using System;

namespace OrderService.Application.DTOs
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
    public class ShopAccountDto
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; } = string.Empty;
    }
}