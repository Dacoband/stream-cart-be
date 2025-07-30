using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBoxService.Application.DTOs
{
    public class ProductSuggestion
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public decimal RelevanceScore { get; set; }
    }
}
