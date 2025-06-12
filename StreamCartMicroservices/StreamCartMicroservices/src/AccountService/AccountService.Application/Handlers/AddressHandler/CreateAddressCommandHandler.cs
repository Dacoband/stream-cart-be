using AccountService.Application.Commands.AddressCommand;
using AccountService.Application.DTOs.Address;
using AccountService.Domain.Entities;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers.AddressHandler
{
    public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IAccountRepository _accountRepository;

        public CreateAddressCommandHandler(
            IAddressRepository addressRepository,
            IAccountRepository accountRepository)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        }

        public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId.ToString());
            if (account == null)
            {
                throw new ApplicationException($"Account with ID {request.AccountId} not found");
            }

            var address = new Address(
                request.AccountId,
                request.RecipientName,
                request.Street,
                request.City,
                request.Country,
                request.PhoneNumber,
                request.Type,
                request.CreatedBy
            );

            address.UpdateAddress(
                request.RecipientName,
                request.Street,
                request.Ward,
                request.District,
                request.City,
                request.Country,
                request.PostalCode,
                request.PhoneNumber
            );

            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                address.UpdateLocation(request.Latitude.Value, request.Longitude.Value);
            }

            if (request.ShopId.HasValue)
            {
                address.AssignToShop(request.ShopId.Value);
            }

            if (request.IsDefaultShipping)
            {
                await _addressRepository.UnsetAllDefaultShippingAddressesAsync(request.AccountId);
                address.SetAsDefaultShipping();
            }

            await _addressRepository.InsertAsync(address);

            var addressDto = new AddressDto
            {
                Id = address.Id,
                RecipientName = address.RecipientName,
                Street = address.Street,
                Ward = address.Ward,
                District = address.District,
                City = address.City,
                Country = address.Country,
                PostalCode = address.PostalCode,
                PhoneNumber = address.PhoneNumber,
                IsDefaultShipping = address.IsDefaultShipping,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                Type = address.Type,
                IsActive = address.IsActive,
                AccountId = address.AccountId,
                ShopId = address.ShopId,
                CreatedAt = address.CreatedAt,
                CreatedBy = address.CreatedBy,
                LastModifiedAt = address.LastModifiedAt,
                LastModifiedBy = address.LastModifiedBy
            };
            return addressDto;
        }
    }
}
