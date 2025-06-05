using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class VerifyPasswordCommand : IRequest<bool>
    {
        public Guid AccountId { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}