using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class ChangePasswordCommand : IRequest<bool>
    {
        public Guid AccountId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}