using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class GenerateOTPCommand : IRequest<string>
    {
        public Guid AccountId { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}