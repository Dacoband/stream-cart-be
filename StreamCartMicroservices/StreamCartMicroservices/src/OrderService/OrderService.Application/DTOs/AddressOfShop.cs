using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class AddressOfShop
    {
        /// <summary>
        /// Name of the shop
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Street address of the shop
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Ward/neighborhood of the shop
        /// </summary>
        public string Ward { get; set; } = string.Empty;

        /// <summary>
        /// District of the shop
        /// </summary>
        public string District { get; set; } = string.Empty;

        /// <summary>
        /// City of the shop (maps to Province in Orders)
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Postal code of the shop
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone number for the shop
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
