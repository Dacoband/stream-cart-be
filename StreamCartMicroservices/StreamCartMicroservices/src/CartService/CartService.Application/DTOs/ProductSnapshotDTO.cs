using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class ProductSnapshotDTO
    {
        public string ProductName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public Guid ShopId { get; set; }

        public decimal PriceCurrent { get; set; }
        public decimal PriceOriginal { get; set; }
        public string PrimaryImage { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = new();
        public int Stock { get; set; }
    }
}
