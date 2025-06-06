using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class GenerateRefreshTokenCommand : IRequest<string>
    {
        public Guid AccountId { get; set; }
    }
}