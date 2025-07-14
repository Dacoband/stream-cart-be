using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.DTOs
{
    public class ProductInLivestreamDTO
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsHighlighted { get; set; }
    }
}
