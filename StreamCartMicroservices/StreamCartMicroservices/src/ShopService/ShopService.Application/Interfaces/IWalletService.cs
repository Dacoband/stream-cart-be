using ShopService.Application.DTOs.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IWalletService
    {
        Task<WalletDTO> GetWalletByIdAsync(Guid id);
        Task<WalletDTO> GetWalletByShopIdAsync(Guid shopId);
        Task<WalletDTO> CreateWalletAsync(CreateWalletDTO createWalletDTO, string createdBy, string shopId);
        Task<WalletDTO> UpdateWalletAsync(Guid id, UpdateWalletDTO updateWalletDTO, string updatedBy);
        Task<bool> ProcessShopPaymentAsync(ShopPaymentDTO paymentRequest);
    }
}
