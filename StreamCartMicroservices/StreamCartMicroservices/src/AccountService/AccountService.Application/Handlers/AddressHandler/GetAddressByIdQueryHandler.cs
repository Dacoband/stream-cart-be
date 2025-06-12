using AccountService.Application.DTOs.Address;
using AccountService.Application.Queries;
using AccountService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers.AddressHandler
{
    public class GetAddressByIdQueryHandler : IRequestHandler<GetAddressByIdQuery, AddressDto>
    {
        private readonly IAddressRepository _addressRepository;

        public GetAddressByIdQueryHandler(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        }

        public async Task<AddressDto> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.Id.ToString());
            if (address == null)
            {
                return null;
            }

            if (address.AccountId != request.AccountId)
            {
                throw new UnauthorizedAccessException($"Address with ID {request.Id} does not belong to account {request.AccountId}");
            }

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
