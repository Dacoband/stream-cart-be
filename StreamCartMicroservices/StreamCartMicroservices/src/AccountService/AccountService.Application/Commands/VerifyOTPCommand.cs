using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class VerifyOTPCommand : IRequest<bool>
    {
        public Guid AccountId { get; set; }
        public string OTP { get; set; } = string.Empty;
    }
}