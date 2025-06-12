using AccountService.Application.DTOs.Address;
using AccountService.Domain.Enums;
using MediatR;
using System;

namespace AccountService.Application.Commands.AddressCommand
{
    public class DeleteAddressCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string DeletedBy { get; set; } = "system";
    }
}
