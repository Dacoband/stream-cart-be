using System;

namespace AccountService.Application.DTOs
{
    public class VerifyOTPDto
    {
        public Guid AccountId { get; set; }
        public string OTP { get; set; } = string.Empty;
    }
}