using System;

namespace AccountService.Application.DTOs
{
    public class ResendOTPDto
    {
        public Guid AccountId { get; set; }
    }
}