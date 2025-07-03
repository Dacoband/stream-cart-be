using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messaging.Event.FlashSaleEvents
{
    public class FlashSaleStartEvent
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public string ProductName { get; set; }
        public decimal Discount { get; set; }
        public string UserId { get; set; }
    }
}
