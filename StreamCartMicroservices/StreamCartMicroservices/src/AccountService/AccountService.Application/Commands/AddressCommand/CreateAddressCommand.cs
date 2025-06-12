using AccountService.Application.DTOs.Address;
using AccountService.Domain.Enums;
using MediatR;
using System;


namespace AccountService.Application.Commands.AddressCommand
{
    public class CreateAddressCommand : IRequest<AddressDto>
    {
        public Guid AccountId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsDefaultShipping { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public AddressType Type { get; set; } = AddressType.Shipping;
        public Guid? ShopId { get; set; }
        public string CreatedBy { get; set; } = "system";
    }
}
