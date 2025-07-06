using AccountService.Application.Commands.AddressCommand;
using AccountService.Application.DTOs.Address;
using AccountService.Application.Interfaces;
using AccountService.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Models;
using Shared.Common.Services.User;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AccountService.Api.Controllers
{
    [Route("api/addresses")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressManagementService _addressService;
        private readonly ICurrentUserService _currentUserService;

        public AddressController(IAddressManagementService addressService, ICurrentUserService currentUserService)
        {
            _addressService = addressService ?? throw new ArgumentNullException(nameof(addressService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        private Guid GetCurrentAccountId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var accountId))
            {
                throw new UnauthorizedAccessException("User identity not found");
            }
            return accountId;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto createAddressDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid address data"));

            try
            {
                var accountId = _currentUserService.GetUserId();

                var command = new CreateAddressCommand
                {
                    AccountId = accountId,
                    RecipientName = createAddressDto.RecipientName,
                    Street = createAddressDto.Street,
                    Ward = createAddressDto.Ward,
                    District = createAddressDto.District,
                    City = createAddressDto.City,
                    Country = createAddressDto.Country,
                    PostalCode = createAddressDto.PostalCode,
                    PhoneNumber = createAddressDto.PhoneNumber,
                    IsDefaultShipping = createAddressDto.IsDefaultShipping,
                    Latitude = createAddressDto.Latitude,
                    Longitude = createAddressDto.Longitude,
                    Type = createAddressDto.Type,
                    ShopId = createAddressDto.ShopId,
                    CreatedBy = accountId.ToString()
                };

                var address = await _addressService.CreateAddressAsync(command);

                return CreatedAtAction(
                    nameof(GetAddressById),
                    new { id = address.Id },
                    ApiResponse<AddressDto>.SuccessResult(address, "Address created successfully")
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error creating address: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateAddressDto updateAddressDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid address data"));

            try
            {
                var accountId = _currentUserService.GetUserId();

                var command = new UpdateAddressCommand
                {
                    Id = id,
                    AccountId = accountId,
                    RecipientName = updateAddressDto.RecipientName,
                    Street = updateAddressDto.Street,
                    Ward = updateAddressDto.Ward,
                    District = updateAddressDto.District,
                    City = updateAddressDto.City,
                    Country = updateAddressDto.Country,
                    PostalCode = updateAddressDto.PostalCode,
                    PhoneNumber = updateAddressDto.PhoneNumber,
                    Type = updateAddressDto.Type,
                    Latitude = updateAddressDto.Latitude,
                    Longitude = updateAddressDto.Longitude,
                    UpdatedBy = accountId.ToString()
                };

                var address = await _addressService.UpdateAddressAsync(command);

                return Ok(ApiResponse<AddressDto>.SuccessResult(address, "Address updated successfully"));
            }
            catch (ApplicationException aex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(aex.Message));
            }
            catch (UnauthorizedAccessException uex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error updating address: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            try
            {
                var accountId = GetCurrentAccountId();
                var result = await _addressService.DeleteAddressAsync(id, accountId);

                return Ok(ApiResponse<bool>.SuccessResult(result, "Address deleted successfully"));
            }
            catch (ApplicationException aex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(aex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error deleting address: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAddressById(Guid id)
        {
            try
            {
                var accountId = _currentUserService.GetUserId();
                var address = await _addressService.GetAddressByIdAsync(id, accountId);

                if (address == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Address not found"));

                return Ok(ApiResponse<AddressDto>.SuccessResult(address));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving address: {ex.Message}"));
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AddressDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddressesByAccountId()
        {
            try
            {
                var accountId = _currentUserService.GetUserId();
                var addresses = await _addressService.GetAddressesByAccountIdAsync(accountId);

                return Ok(ApiResponse<IEnumerable<AddressDto>>.SuccessResult(addresses));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving addresses: {ex.Message}"));
            }
        }

        [HttpGet("shops/{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AddressDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddressesByShopId(Guid shopId)
        {
            try
            {
                var addresses = await _addressService.GetAddressesByShopIdAsync(shopId);
                return Ok(ApiResponse<IEnumerable<AddressDto>>.SuccessResult(addresses));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving shop addresses: {ex.Message}"));
            }
        }

        [HttpGet("default-shipping")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDefaultShippingAddress()
        {
            try
            {
                var accountId = _currentUserService.GetUserId();
                var address = await _addressService.GetDefaultShippingAddressAsync(accountId);

                if (address == null)
                    return NotFound(ApiResponse<object>.ErrorResult("Default shipping address not found"));

                return Ok(ApiResponse<AddressDto>.SuccessResult(address));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving default shipping address: {ex.Message}"));
            }
        }

        [HttpGet("by-type/{type}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AddressDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddressesByType(AddressType type)
        {
            try
            {
                var accountId = _currentUserService.GetUserId();
                var addresses = await _addressService.GetAddressesByTypeAsync(accountId, type);

                return Ok(ApiResponse<IEnumerable<AddressDto>>.SuccessResult(addresses));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error retrieving addresses by type: {ex.Message}"));
            }
        }

        [HttpPut("{id}/set-default-shipping")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetDefaultShippingAddress(Guid id)
        {
            try
            {
                var accountId = _currentUserService.GetUserId();
                var result = await _addressService.SetDefaultShippingAddressAsync(id, accountId);

                if (!result)
                    return BadRequest(ApiResponse<object>.ErrorResult("Failed to set default shipping address"));

                return Ok(ApiResponse<bool>.SuccessResult(true, "Default shipping address set successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error setting default shipping address: {ex.Message}"));
            }
        }

        [HttpPut("{id}/assign-to-shop/{shopId}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignAddressToShop(Guid id, Guid shopId)
        {
            try
            {
                var accountId = _currentUserService.GetUserId();
                var address = await _addressService.AssignAddressToShopAsync(id, accountId, shopId);

                return Ok(ApiResponse<AddressDto>.SuccessResult(address, "Address assigned to shop successfully"));
            }
            catch (ApplicationException aex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(aex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error assigning address to shop: {ex.Message}"));
            }
        }

        [HttpPut("{id}/unassign-from-shop")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UnassignAddressFromShop(Guid id)
        {
            try
            {
                var accountId = _currentUserService.GetUserId();
                var address = await _addressService.UnassignAddressFromShopAsync(id, accountId);

                return Ok(ApiResponse<AddressDto>.SuccessResult(address, "Address unassigned from shop successfully"));
            }
            catch (ApplicationException aex)
            {
                return NotFound(ApiResponse<object>.ErrorResult(aex.Message));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResult($"Error unassigning address from shop: {ex.Message}"));
            }
        }
    }
}