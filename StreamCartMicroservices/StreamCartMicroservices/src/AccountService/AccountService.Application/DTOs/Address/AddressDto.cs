using AccountService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs.Address
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public required string RecipientName { get; set; }
        public required string Street { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDefaultShipping { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public AddressType Type { get; set; }
        public bool IsActive { get; set; }
        public Guid AccountId { get; set; }
        public Guid? ShopId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }
}
