using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.DTOs
{
    public class UpdateLivestreamProductDTO
    {
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public bool? IsPin { get; set; }
        public Guid? FlashSaleId { get; set; }
    }
}
