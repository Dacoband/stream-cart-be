using MediatR;
using System;

namespace AccountService.Application.Commands
{
    public class UpdateLastLoginCommand : IRequest<bool>
    {
        public Guid AccountId { get; set; }
    }
}