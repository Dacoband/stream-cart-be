using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Products
{
    public class ShopDetailDto
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoURL { get; set; } = string.Empty;
        public string CoverImageURL { get; set; } = string.Empty;
        public decimal RatingAverage { get; set; }
        public int TotalReview { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalProduct { get; set; }
        public decimal CompleteRate { get; set; }
    }
}
