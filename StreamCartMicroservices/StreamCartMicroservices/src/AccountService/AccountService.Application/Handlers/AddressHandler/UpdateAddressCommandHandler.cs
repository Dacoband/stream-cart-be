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
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, AddressDto>
    {
        private readonly IAddressRepository _addressRepository;

        public UpdateAddressCommandHandler(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        }

        public async Task<AddressDto> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.Id.ToString());
            if (address == null)
            {
                throw new ApplicationException($"Address with ID {request.Id} not found");
            }

            if (address.AccountId != request.AccountId)
            {
                throw new UnauthorizedAccessException($"Address with ID {request.Id} does not belong to account {request.AccountId}");
            }

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

            if (request.Type.HasValue)
            {
                address.UpdateType(request.Type.Value);
            }

            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                address.UpdateLocation(request.Latitude.Value, request.Longitude.Value);
            }

            address.SetUpdatedBy(request.UpdatedBy);

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
