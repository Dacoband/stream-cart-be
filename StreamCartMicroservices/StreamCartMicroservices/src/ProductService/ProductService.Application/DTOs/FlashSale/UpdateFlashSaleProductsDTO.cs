using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.FlashSale
{
    public class UpdateFlashSaleProductsDTO
    {
        public List<Guid> ProductIds { get; set; } = new();
        public List<Guid>? VariantIds { get; set; }
    }
}
