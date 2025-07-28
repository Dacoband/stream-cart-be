using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.DTOs.LiveStreamProduct
{
    public class FlashSaleDTO
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public decimal FlashSalePrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
