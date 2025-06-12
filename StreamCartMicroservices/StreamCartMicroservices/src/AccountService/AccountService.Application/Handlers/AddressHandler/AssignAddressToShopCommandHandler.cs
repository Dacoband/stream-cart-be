using AccountService.Application.Commands.AddressCommand;
using AccountService.Application.DTOs.Address;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers.AddressHandler
{
    public class AssignAddressToShopCommandHandler : IRequestHandler<AssignAddressToShopCommand, AddressDto>
    {
        private readonly IAddressRepository _addressRepository;

        public AssignAddressToShopCommandHandler(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        }

        public async Task<AddressDto> Handle(AssignAddressToShopCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.AddressId.ToString());
            if (address == null)
            {
                throw new ApplicationException($"Address with ID {request.AddressId} not found");
            }

            if (address.AccountId != request.AccountId)
            {
                throw new UnauthorizedAccessException($"Address with ID {request.AddressId} does not belong to account {request.AccountId}");
            }

            address.AssignToShop(request.ShopId);

            await _addressRepository.ReplaceAsync(address.Id.ToString(), address);

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
