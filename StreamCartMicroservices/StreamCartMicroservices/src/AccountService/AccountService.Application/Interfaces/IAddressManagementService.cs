using AccountService.Application.Commands.AddressCommand;
using AccountService.Application.DTOs.Address;
using AccountService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountService.Application.Interfaces
{
    public interface IAddressManagementService
    {
        /// <summary>
        /// T?o ??a ch? m?i cho tài kho?n
        /// </summary>
        Task<AddressDto> CreateAddressAsync(CreateAddressCommand command);
        
        /// <summary>
        /// C?p nh?t thông tin ??a ch?
        /// </summary>
        Task<AddressDto> UpdateAddressAsync(UpdateAddressCommand command);
        
        /// <summary>
        /// Xóa ??a ch?
        /// </summary>
        Task<bool> DeleteAddressAsync(Guid addressId, Guid accountId);
        
        /// <summary>
        /// L?y ??a ch? theo ID
        /// </summary>
        Task<AddressDto> GetAddressByIdAsync(Guid addressId, Guid accountId);
        
        /// <summary>
        /// L?y danh sách ??a ch? theo tài kho?n
        /// </summary>
        Task<IEnumerable<AddressDto>> GetAddressesByAccountIdAsync(Guid accountId);
        
        /// <summary>
        /// L?y danh sách ??a ch? theo c?a hàng
        /// </summary>
        Task<IEnumerable<AddressDto>> GetAddressesByShopIdAsync(Guid shopId);
        
        /// <summary>
        /// L?y ??a ch? giao hàng m?c ??nh c?a tài kho?n
        /// </summary>
        Task<AddressDto> GetDefaultShippingAddressAsync(Guid accountId);
        
        /// <summary>
        /// L?y danh sách ??a ch? theo lo?i
        /// </summary>
        Task<IEnumerable<AddressDto>> GetAddressesByTypeAsync(Guid accountId, AddressType type);
        
        /// <summary>
        /// ??t ??a ch? làm ??a ch? giao hàng m?c ??nh
        /// </summary>
        Task<bool> SetDefaultShippingAddressAsync(Guid addressId, Guid accountId);
        
        /// <summary>
        /// Gán ??a ch? cho c?a hàng
        /// </summary>
        Task<AddressDto> AssignAddressToShopAsync(Guid addressId, Guid accountId, Guid shopId);
        
        /// <summary>
        /// H?y gán ??a ch? kh?i c?a hàng
        /// </summary>
        Task<AddressDto> UnassignAddressFromShopAsync(Guid addressId, Guid accountId);
    }
}