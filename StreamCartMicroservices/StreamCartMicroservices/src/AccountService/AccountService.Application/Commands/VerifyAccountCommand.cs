using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class VerifyAccountCommand : IRequest<bool>
    {
        public Guid AccountId { get; set; }
        public string VerificationToken { get; set; } = string.Empty;
    }
}