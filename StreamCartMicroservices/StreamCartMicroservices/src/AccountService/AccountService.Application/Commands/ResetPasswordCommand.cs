using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public Guid AccountId { get; set; }
        public string ResetToken { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}