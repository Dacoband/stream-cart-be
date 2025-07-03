using System;
using System.ComponentModel.DataAnnotations;

namespace ShopService.Application.DTOs
{
    public class UpdateCompletionRateDto
    {
        [Required]
        [Range(-100, 100, ErrorMessage = "Rate change must be between -100 and 100")]
        public decimal RateChange { get; set; }

        [Required]
        public Guid UpdatedByAccountId { get; set; }
    }
}