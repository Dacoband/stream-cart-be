using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Event.ProductEvent
{
    public class ProductUpdatedEvent
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string? ProductName { get; set; }
        public string? PrimaryImage { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public bool? ProductStatus { get; set; }
    }
}
