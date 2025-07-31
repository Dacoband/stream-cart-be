using Microsoft.Extensions.Logging;
using ShopService.Application.DTOs.Wallet;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger<WalletService> _logger;
        private readonly IShopRepository _shopRepository;

        public WalletService(
            IWalletRepository walletRepository,
            IShopRepository shopRepository,
            ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _shopRepository = shopRepository;
            _logger = logger;
        }

        public async Task<WalletDTO> CreateWalletAsync(CreateWalletDTO createWalletDTO, string createdBy, string ShopId)
        {
            try
            {
                var wallet = new Wallet
                {
                    OwnerType = createWalletDTO.OwnerType ?? "Shop",
                    Balance = 0,
                    BankName = createWalletDTO.BankName ?? string.Empty,
                    BankAccountNumber = createWalletDTO.BankAccountNumber ?? string.Empty,
                    ShopId = Guid.Parse(ShopId),
                    CreatedAt = DateTime.UtcNow
                };

                wallet.SetCreator(createdBy);

                await _walletRepository.InsertAsync(wallet);

              

                return MapToDto(wallet);
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        public async Task<WalletDTO> GetWalletByIdAsync(Guid id)
        {
            try
            {
                var wallet = await _walletRepository.GetByIdAsync(id.ToString());
                if (wallet == null)
                {
                    _logger.LogWarning("Không tìm thấy ví với ID {WalletId}", id);
                    return null;
                }

                return MapToDto(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin ví {WalletId}", id);
                throw;
            }
        }

        public async Task<WalletDTO> GetWalletByShopIdAsync(Guid shopId)
        {
            try
            {
                // Fix: Sử dụng GetByShopIdAsync thay vì FindOneAsync
                var wallet = await _walletRepository.GetByShopIdAsync(shopId);
                if (wallet == null)
                {
                    _logger.LogWarning("Không tìm thấy ví cho shop {ShopId}", shopId);
                    return null;
                }

                return MapToDto(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin ví của shop {ShopId}", shopId);
                throw;
            }
        }

        public async Task<WalletDTO> UpdateWalletAsync(Guid id, UpdateWalletDTO updateWalletDTO, string updatedBy)
        {
            try
            {
                var wallet = await _walletRepository.GetByIdAsync(id.ToString());
                if (wallet == null)
                {
                    _logger.LogWarning("Không tìm thấy ví với ID {WalletId} để cập nhật", id);
                    return null;
                }

                // Cập nhật thông tin ngân hàng
                wallet.BankName = updateWalletDTO.BankName ?? wallet.BankName;
                wallet.BankAccountNumber = updateWalletDTO.BankAccountNumber ?? wallet.BankAccountNumber;
                wallet.UpdatedAt = DateTime.UtcNow;
                wallet.SetModifier(updatedBy);

                await _walletRepository.ReplaceAsync(wallet.Id.ToString(), wallet);

                _logger.LogInformation("Đã cập nhật thông tin ngân hàng cho ví {WalletId}", id);

                return MapToDto(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin ví {WalletId}", id);
                throw;
            }
        }

        public async Task<bool> ProcessShopPaymentAsync(ShopPaymentDTO paymentRequest)
        {
            try
            {
                // Kiểm tra shop có tồn tại không
                var shop = await _shopRepository.GetByIdAsync(paymentRequest.ShopId.ToString());
                if (shop == null)
                {
                    _logger.LogWarning("Không thể thanh toán: Shop {ShopId} không tồn tại", paymentRequest.ShopId);
                    return false;
                }

                // Fix: Sử dụng GetByShopIdAsync thay vì FindOneAsync
                var wallet = await _walletRepository.GetByShopIdAsync(paymentRequest.ShopId);
                if (wallet == null)
                {
                    _logger.LogWarning("Không thể thanh toán: Shop {ShopId} chưa có ví", paymentRequest.ShopId);

                    // Tự động tạo ví cho shop nếu chưa có
                    wallet = new Wallet
                    {
                        OwnerType = "Shop",
                        Balance = 0,
                        BankName = shop.BankName,
                        BankAccountNumber = shop.BankAccountNumber,
                        ShopId = shop.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    wallet.SetCreator("System");

                    await _walletRepository.InsertAsync(wallet);
                    _logger.LogInformation("Đã tự động tạo ví mới cho shop {ShopId}", paymentRequest.ShopId);
                }

                // Cộng tiền vào ví
                wallet.AddFunds(paymentRequest.Amount, "System");
                await _walletRepository.ReplaceAsync(wallet.Id.ToString(), wallet);

                // Ghi log giao dịch
                _logger.LogInformation(
                    "Đã thanh toán {Amount} cho shop {ShopId} từ đơn hàng {OrderId} (phí: {Fee})",
                    paymentRequest.Amount,
                    paymentRequest.ShopId,
                    paymentRequest.OrderId,
                    paymentRequest.Fee);

                // Cập nhật tỷ lệ hoàn thành của shop (+0.5%)
                shop.UpdateCompleteRate(Math.Min(shop.CompleteRate + 0.5m, 100), "System");
                await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thanh toán cho shop {ShopId}, đơn hàng {OrderId}",
                    paymentRequest.ShopId, paymentRequest.OrderId);
                throw;
            }
        }

        private static WalletDTO MapToDto(Wallet wallet)
        {
            return new WalletDTO
            {
                Id = wallet.Id,
                OwnerType = wallet.OwnerType,
                Balance = wallet.Balance,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt,
                BankName = wallet.BankName,
                BankAccountNumber = wallet.BankAccountNumber,
                ShopId = wallet.ShopId
            };
        }
    }
}