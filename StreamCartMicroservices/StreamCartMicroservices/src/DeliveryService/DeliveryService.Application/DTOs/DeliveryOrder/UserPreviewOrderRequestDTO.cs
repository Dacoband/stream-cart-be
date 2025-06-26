using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.DeliveryOrder
{
    public class UserPreviewOrderRequestDTO
    {
        public string FromProvince { get; set; }
        public string FromDistrict { get; set; }
        public string FromWard { get; set; }
        public string ToProvince { get; set; }
        public string ToDistrict { get; set; }
        public string ToWard { get; set; }
        public List<UserOrderItem> Items { get; set; } = new();


    }
}
