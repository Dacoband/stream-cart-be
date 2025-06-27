using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.DeliveryOrder
{
    public class OrderLogResponse
    {
        public List<OrderLogItem> Logs { get; set; } = new();
    }

    public class OrderLogItem
    {
        public string Status { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
