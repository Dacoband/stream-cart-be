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
    public class GetDefaultShippingAddressQueryHandler : IRequestHandler<GetDefaultShippingAddressQuery, AddressDto>
    {
        private readonly IAddressRepository _addressRepository;

        public GetDefaultShippingAddressQueryHandler(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        }

        public async Task<AddressDto> Handle(GetDefaultShippingAddressQuery request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetDefaultShippingAddressByAccountIdAsync(request.AccountId);

            if (address == null)
            {
                return null;
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
