using AccountService.Application.Commands.AddressCommand;
using AccountService.Application.DTOs.Address;
using AccountService.Application.Interfaces;
using AccountService.Application.Queries;
using AccountService.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountService.Application.Services
{
    public class AddressManagementService : IAddressManagementService
    {
        private readonly IMediator _mediator;

        public AddressManagementService(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<AddressDto> CreateAddressAsync(CreateAddressCommand command)
        {
            return await _mediator.Send(command);
        }

        public async Task<AddressDto> UpdateAddressAsync(UpdateAddressCommand command)
        {
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteAddressAsync(Guid addressId, Guid accountId)
        {
            var command = new DeleteAddressCommand
            {
                Id = addressId,
                AccountId = accountId,
                DeletedBy = accountId.ToString()
            };
            return await _mediator.Send(command);
        }

        public async Task<AddressDto> GetAddressByIdAsync(Guid addressId, Guid accountId)
        {
            var query = new GetAddressByIdQuery
            {
                Id = addressId,
                AccountId = accountId
            };
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesByAccountIdAsync(Guid accountId)
        {
            var query = new GetAddressesByAccountIdQuery
            {
                AccountId = accountId
            };
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesByShopIdAsync(Guid shopId)
        {
            var query = new GetAddressesByShopIdQuery
            {
                ShopId = shopId
            };
            return await _mediator.Send(query);
        }

        public async Task<AddressDto> GetDefaultShippingAddressAsync(Guid accountId)
        {
            var query = new GetDefaultShippingAddressQuery
            {
                AccountId = accountId
            };
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesByTypeAsync(Guid accountId, AddressType type)
        {
            var query = new GetAddressesByTypeQuery
            {
                AccountId = accountId,
                Type = type
            };
            return await _mediator.Send(query);
        }

        public async Task<bool> SetDefaultShippingAddressAsync(Guid addressId, Guid accountId)
        {
            var command = new SetDefaultShippingAddressCommand
            {
                AddressId = addressId,
                AccountId = accountId
            };
            return await _mediator.Send(command);
        }

        public async Task<AddressDto> AssignAddressToShopAsync(Guid addressId, Guid accountId, Guid shopId)
        {
            var command = new AssignAddressToShopCommand
            {
                AddressId = addressId,
                AccountId = accountId,
                ShopId = shopId
            };
            return await _mediator.Send(command);
        }

        public async Task<AddressDto> UnassignAddressFromShopAsync(Guid addressId, Guid accountId)
        {
            var command = new UnassignAddressFromShopCommand
            {
                AddressId = addressId,
                AccountId = accountId
            };
            return await _mediator.Send(command);
        }
    }
}