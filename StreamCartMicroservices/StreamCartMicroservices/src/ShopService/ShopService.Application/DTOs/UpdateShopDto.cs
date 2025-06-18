using System;

namespace ShopService.Application.DTOs
{
    public class UpdateShopDto
    {
        public string? ShopName { get; set; }
        public string? Description { get; set; }
        public string? LogoURL { get; set; }
        public string? CoverImageURL { get; set; }
    }
}