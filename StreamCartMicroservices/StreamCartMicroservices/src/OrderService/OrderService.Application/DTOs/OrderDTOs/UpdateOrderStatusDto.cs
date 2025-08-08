using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs.OrderDTOs
{
    public class UpdateOrderStatusDto
    {
        public Guid OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
