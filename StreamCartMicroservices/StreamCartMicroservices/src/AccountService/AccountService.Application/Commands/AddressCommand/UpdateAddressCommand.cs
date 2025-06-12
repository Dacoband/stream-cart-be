using AccountService.Application.DTOs.Address;
using AccountService.Domain.Enums;
using MediatR;
using System;

namespace AccountService.Application.Commands.AddressCommand
{
    public class UpdateAddressCommand : IRequest<AddressDto>
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string RecipientName { get; set; }
        public string Street { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public AddressType? Type { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string UpdatedBy { get; set; } = "system";
    }
}
