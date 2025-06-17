using System;

namespace ShopService.Application.DTOs
{
    public class CreateShopDto
    {
        public string ShopName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoURL { get; set; } = string.Empty;
        public string CoverImageURL { get; set; } = string.Empty;
    }
}