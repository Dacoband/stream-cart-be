using OrderService.Application.DTOs.DeliveryDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    public interface IDeliveryApiClient
    {
        /// <summary>
        /// Gets order log from delivery API using tracking code
        /// </summary>
        /// <param name="trackingCode">Tracking code of the order</param>
        /// <returns>Order log response</returns>
        Task<DeliveryApiResponse?> GetOrderLogAsync(string trackingCode);
    }
}
