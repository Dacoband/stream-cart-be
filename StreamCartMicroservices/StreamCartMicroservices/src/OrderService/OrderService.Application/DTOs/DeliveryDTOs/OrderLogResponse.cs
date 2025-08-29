using System;
using System.Collections.Generic;

namespace OrderService.Application.DTOs.DeliveryDTOs
{
    public class OrderLogResponse
    {
        public List<OrderLogItem> Logs { get; set; } = new List<OrderLogItem>();
    }

    public class OrderLogItem
    {
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
    }

    public class DeliveryApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public OrderLogResponse Data { get; set; } = new OrderLogResponse();
        public List<string> Errors { get; set; } = new List<string>();
    }
}