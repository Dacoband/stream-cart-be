using DeliveryService.Application.DTOs.AccountDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.Interfaces
{
    public interface IAddressClientService
    {
        Task<AddressDto?> GetShopAddress(string shopId);
    }
}
