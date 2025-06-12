using AccountService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.DTOs.Address
{
    public class UpdateAddressDto
    {
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
    }
}
