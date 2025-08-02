using OrderService.Application.DTOs.Delivery;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    public interface IDeliveryClient
    {
         Task<ApiResponse<object>> CreateGhnOrderAsync(UserCreateOrderRequest request);
        Task<ApiResponse<bool>> CancelDeliveryOrderAsync(string deliveryCode);
    }
}
