using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Order
{
    public class PreviewOrderRequestDTO
    {
        public string? ProductId { get; set; }
        public string? VariantId { get; set; }
        public string? LivestreamProductId { get; set; }
    }
}
