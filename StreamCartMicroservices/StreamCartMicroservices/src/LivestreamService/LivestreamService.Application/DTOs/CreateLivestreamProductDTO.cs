using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.DTOs
{
    public class CreateLivestreamProductDTO
    {
        public Guid LivestreamId { get; set; }
        public string? ProductId { get; set; }
        public string? VariantId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsPin { get; set; }
       // public Guid? FlashSaleId { get; set; }
    }
}
