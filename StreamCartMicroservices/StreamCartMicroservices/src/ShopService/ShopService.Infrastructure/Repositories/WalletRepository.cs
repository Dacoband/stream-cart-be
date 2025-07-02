using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Common.Data.Repositories;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly ShopContext _context;
        private readonly ILogger<WalletRepository> _logger;

        public WalletRepository(
            ShopContext context,
            ILogger<WalletRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Wallet> GetByIdAsync(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid walletId))
                {
                    _logger.LogWarning("ID ví không hợp lệ: {WalletId}", id);
                    return null;
                }

                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.Id == walletId && !w.IsDeleted);

                if (wallet == null)
                {
                    _logger.LogInformation("Không tìm thấy ví với ID: {WalletId}", id);
                }

                return wallet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy ví theo ID {WalletId}", id);
                throw;
            }
        }

        public async Task<Wallet> GetByShopIdAsync(Guid shopId)
        {
            try
            {
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.ShopId == shopId && !w.IsDeleted);

                if (wallet == null)
                {
                    _logger.LogInformation("Không tìm thấy ví cho shop {ShopId}", shopId);
                }

                return wallet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy ví theo shop ID {ShopId}", shopId);
                throw;
            }
        }

        public async Task<Wallet> InsertAsync(Wallet wallet)
        {
            try
            {
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã tạo ví mới với ID {WalletId} cho shop {ShopId}",
                    wallet.Id, wallet.ShopId);

                return wallet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo ví mới cho shop {ShopId}", wallet.ShopId);
                throw;
            }
        }

        public async Task<bool> ReplaceAsync(string id, Wallet wallet)
        {
            try
            {
                if (!Guid.TryParse(id, out Guid walletId) || walletId != wallet.Id)
                {
                    _logger.LogWarning("ID ví không hợp lệ hoặc không khớp: {WalletId}", id);
                    return false;
                }

                // Đánh dấu entity là đang được theo dõi và đã sửa đổi
                _context.Entry(wallet).State = EntityState.Modified;

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật ví {WalletId}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi về tính đồng thời khi cập nhật ví {WalletId}", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật ví {WalletId}", id);
                return false;
            }
        }

        public async Task<bool> AddFundsAsync(string walletId, decimal amount, string modifiedBy)
        {
            try
            {
                var wallet = await GetByIdAsync(walletId);
                if (wallet == null)
                {
                    _logger.LogWarning("Không tìm thấy ví {WalletId} để thêm tiền", walletId);
                    return false;
                }

                // Thêm tiền vào ví
                wallet.AddFunds(amount, modifiedBy);

                // Cập nhật ví trong database
                return await ReplaceAsync(walletId, wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm tiền vào ví {WalletId}", walletId);
                return false;
            }
        }
    }
}