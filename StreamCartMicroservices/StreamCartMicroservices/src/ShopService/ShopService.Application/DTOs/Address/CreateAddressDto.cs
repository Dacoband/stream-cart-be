using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Address
{
    public class CreateAddressDto
    {
        public string RecipientName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsDefaultShipping { get; set; } = true;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public AddressType Type { get; set; } = AddressType.Business;
        public Guid? ShopId { get; set; }
    }
}
