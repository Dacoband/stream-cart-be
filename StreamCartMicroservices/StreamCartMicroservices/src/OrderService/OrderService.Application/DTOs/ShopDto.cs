using System;

namespace OrderService.Application.DTOs
{

    public class ShopDto
    {
        public Guid Id { get; set; }

        public string ShopName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string LogoURL { get; set; } = string.Empty;

        public string CoverImageURL { get; set; } = string.Empty;

        public Guid AccountId { get; set; }

        public bool Status { get; set; }

        public string ApprovalStatus { get; set; } = string.Empty;
    }
}