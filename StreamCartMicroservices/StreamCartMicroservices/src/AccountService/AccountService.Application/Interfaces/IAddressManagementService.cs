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
        /// T?o ??a ch? m?i cho t�i kho?n
        /// </summary>
        Task<AddressDto> CreateAddressAsync(CreateAddressCommand command);
        
        /// <summary>
        /// C?p nh?t th�ng tin ??a ch?
        /// </summary>
        Task<AddressDto> UpdateAddressAsync(UpdateAddressCommand command);
        
        /// <summary>
        /// X�a ??a ch?
        /// </summary>
        Task<bool> DeleteAddressAsync(Guid addressId, Guid accountId);
        
        /// <summary>
        /// L?y ??a ch? theo ID
        /// </summary>
        Task<AddressDto> GetAddressByIdAsync(Guid addressId, Guid accountId);
        
        /// <summary>
        /// L?y danh s�ch ??a ch? theo t�i kho?n
        /// </summary>
        Task<IEnumerable<AddressDto>> GetAddressesByAccountIdAsync(Guid accountId);
        
        /// <summary>
        /// L?y danh s�ch ??a ch? theo c?a h�ng
        /// </summary>
        Task<IEnumerable<AddressDto>> GetAddressesByShopIdAsync(Guid shopId);
        
        /// <summary>
        /// L?y ??a ch? giao h�ng m?c ??nh c?a t�i kho?n
        /// </summary>
        Task<AddressDto> GetDefaultShippingAddressAsync(Guid accountId);
        
        /// <summary>
        /// L?y danh s�ch ??a ch? theo lo?i
        /// </summary>
        Task<IEnumerable<AddressDto>> GetAddressesByTypeAsync(Guid accountId, AddressType type);
        
        /// <summary>
        /// ??t ??a ch? l�m ??a ch? giao h�ng m?c ??nh
        /// </summary>
        Task<bool> SetDefaultShippingAddressAsync(Guid addressId, Guid accountId);
        
        /// <summary>
        /// G�n ??a ch? cho c?a h�ng
        /// </summary>
        Task<AddressDto> AssignAddressToShopAsync(Guid addressId, Guid accountId, Guid shopId);
        
        /// <summary>
        /// H?y g�n ??a ch? kh?i c?a h�ng
        /// </summary>
        Task<AddressDto> UnassignAddressFromShopAsync(Guid addressId, Guid accountId);
    }
}