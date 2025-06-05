using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class RequestPasswordResetCommand : IRequest<bool>
    {
        public Guid AccountId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}