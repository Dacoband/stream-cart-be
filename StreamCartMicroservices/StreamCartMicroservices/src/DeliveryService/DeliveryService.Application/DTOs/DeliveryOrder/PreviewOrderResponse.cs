using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.DeliveryOrder
{
    public class PreviewOrderResponse
    {
        public List<ServiceResponse> ServiceResponses { get; set; }
    }
    public class ServiceResponse
    {
        public int ServiceTypeId { get; set; }
        public string ServiceName { get; set; }
        public int TotalAmount { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }

    }
}
