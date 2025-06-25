using DeliveryService.Application.DTOs.AddressDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryService.Application.Interfaces
{
    public interface IDeliveryAddressInterface
    {
        Task<int?> FindProvinceIdByNameAsync(string provinceName);
        Task<int?> FindDistrictIdByNameAsync(string districtName, int provinceId);
        Task<string?> FindWardCodeByNameAsync(string wardName, int districtId);
        public Task<List<GHNProvinceDTO>> GetProvincesAsync();
        public Task<List<GHNDistrictDTO>> GetDistrictsAsync(int provinceId  );
        Task<List<GHNWardDTO>> GetWardsAsync(int districtId);

    }
}
