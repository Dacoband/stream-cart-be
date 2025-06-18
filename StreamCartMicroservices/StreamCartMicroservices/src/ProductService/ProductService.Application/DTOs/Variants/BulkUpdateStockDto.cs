using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Variants
{
    public class BulkUpdateStockDto
    {
        public List<VariantStockUpdate> StockUpdates { get; set; } = new List<VariantStockUpdate>();
    }
}
