using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class AdressDto
    {
        public Guid Id { get; set; }
        public required string RecipientName { get; set; }
        public required string Street { get; set; }
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsDefaultShipping { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

    }
}
