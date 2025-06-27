using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.DeliveryOrder
{
    public class CreateOrderResult
    {
        public string DeliveryId { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
    }
}
