using OrderService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    public interface IShopVoucherClientService
    {
        Task<VoucherValidationDto> ValidateVoucherAsync(string code, decimal orderAmount, Guid? shopId = null);
        Task<VoucherApplicationDto> ApplyVoucherAsync(string code, Guid orderId, decimal orderAmount, string accessToken);
    }
}
