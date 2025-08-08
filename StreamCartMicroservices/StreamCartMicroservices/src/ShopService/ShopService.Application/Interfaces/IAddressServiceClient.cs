using Shared.Common.Models;
using ShopService.Application.DTOs.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IAddressServiceClient
    {
        Task<ApiResponse<AddressDto>> CreateAddressAsync(CreateAddressDto dto, string token);
        Task<ApiResponse<AddressDto>> UpdateAddressAsync(Guid id, CreateAddressDto dto, string token);
        Task<ApiResponse<AddressDto>> GetAddressByIdAsync(Guid id, string token);
        Task<ApiResponse<IEnumerable<AddressDto>>> GetAddressesByShopIdAsync(Guid shopId);
    }
}
