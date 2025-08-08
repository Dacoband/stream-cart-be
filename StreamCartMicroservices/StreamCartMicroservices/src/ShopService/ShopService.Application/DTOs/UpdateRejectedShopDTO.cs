using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs
{
    public class UpdateRejectedShopDTO
    {
        public string? ShopName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public string? LogoURL { get; set; } = string.Empty;
        public string? CoverImageURL { get; set; } = string.Empty;
        public string? Street { get; set; } = string.Empty;
        public string? Ward { get; set; } = string.Empty;
        public string? District { get; set; } = string.Empty;
        public string? City { get; set; } = string.Empty;
        public string? Country { get; set; } = string.Empty;
        public string? PostalCode { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; } = string.Empty;
        public string AccessToken {  get; set; } = string.Empty;
        public string? BankName { get; set; } = string.Empty;
        public string? BankNumber { get; set; } = string.Empty;
        public string? Tax { get; set; } = string.Empty;
    }
}
