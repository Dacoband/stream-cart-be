using DeliveryService.Application.DTOs.AddressDTOs;
using DeliveryService.Application.DTOs.DeliveryOrder;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.Interfaces
{
    public interface IDeliveryAddressInterface
    {
        public Task<ApiResponse<CreateOrderResult>> CreateOrderAsync(UserCreateOrderRequest input);
        public Task<ApiResponse<PreviewOrderResponse>> PreviewOrder(UserPreviewOrderRequestDTO input);
        public Task<ApiResponse<OrderLogResponse>> GetDeliveryStatus(string deliveryId);
        public Task<string> CancelDeliveryOrder(string deliveryId);

    }
}
