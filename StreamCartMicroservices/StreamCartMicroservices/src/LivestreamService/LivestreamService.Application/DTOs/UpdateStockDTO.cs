using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.DTOs
{
    public class UpdateStockDTO
    {
        public int Stock { get; set; }
        public decimal? Price { get; set; }

    }
}
