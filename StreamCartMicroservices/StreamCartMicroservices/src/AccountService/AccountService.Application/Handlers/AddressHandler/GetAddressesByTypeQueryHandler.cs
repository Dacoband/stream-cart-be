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
    public class GetAddressesByTypeQueryHandler : IRequestHandler<GetAddressesByTypeQuery, IEnumerable<AddressDto>>
    {
        private readonly IAddressRepository _addressRepository;

        public GetAddressesByTypeQueryHandler(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        }

        public async Task<IEnumerable<AddressDto>> Handle(GetAddressesByTypeQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetAddressesByTypeAsync(request.AccountId, request.Type);

            return addresses.Select(address => new AddressDto
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
            }).ToList();
        }
    }
}
