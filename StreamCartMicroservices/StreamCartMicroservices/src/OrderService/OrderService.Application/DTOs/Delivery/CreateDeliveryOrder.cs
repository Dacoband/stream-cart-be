using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs.Delivery
{
    public class UserCreateOrderRequest
    {
        // Người nhận
        public string ToName { get; set; }
        public string ToPhone { get; set; }
        public string ToProvince { get; set; }
        public string ToDistrict { get; set; }
        public string ToWard { get; set; }
        public string ToAddress { get; set; }

        // Người gửi
        public string FromName { get; set; }
        public string FromPhone { get; set; }
        public string FromProvince { get; set; }
        public string FromDistrict { get; set; }
        public string FromWard { get; set; }
        public string FromAddress { get; set; }

        public int ServiceTypeId { get; set; }


        // Ghi chú đơn hàng
        public string? Note { get; set; }
        public string? Description { get; set; }
        public int? CodAmount { get; set; }

        // Sản phẩm
        public List<UserOrderItem> Items { get; set; } = new();
    }

    public class UserOrderItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Weight { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
