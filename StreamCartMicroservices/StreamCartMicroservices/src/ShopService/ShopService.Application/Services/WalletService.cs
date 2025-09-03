using Microsoft.Extensions.Logging;
using ShopService.Application.DTOs.Wallet;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger<WalletService> _logger;
        private readonly IShopRepository _shopRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public WalletService(
            IWalletRepository walletRepository,
            IShopRepository shopRepository,
            IWalletTransactionRepository walletTransactionRepository,
            ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _shopRepository = shopRepository;
            _walletTransactionRepository = walletTransactionRepository;
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
                var walletTransaction = await _walletTransactionRepository.GetAllAsync();
                walletTransaction =  walletTransaction.Where(x => x.WalletId == id && x.Status == WalletTransactionStatus.Pending.ToString() && x.IsDeleted != false).ToList();

                
                await _walletRepository.ReplaceAsync(wallet.Id.ToString(), wallet);
                if(walletTransaction.Count() > 0)
                {
                    foreach (var transaction in walletTransaction)
                    {
                        transaction.BankAccount = wallet.BankName;
                        transaction.BankNumber = wallet.BankAccountNumber;
                        transaction.SetModifier("system");
                        await _walletTransactionRepository.ReplaceAsync(transaction.Id.ToString(), transaction);

                    }
                }
              

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
                    return false;
                }

                // Cộng tiền vào ví
                wallet.AddFunds(paymentRequest.Amount, "System");
                await _walletRepository.ReplaceAsync(wallet.Id.ToString(), wallet);
                var transaction = new WalletTransaction()
                {
                    Amount = paymentRequest.Amount,
                    BankAccount = wallet.BankName,
                    BankNumber = wallet.BankAccountNumber,
                    Description = $"Thanh toán đơn hàng {paymentRequest.OrderId}",
                    OrderId = paymentRequest.OrderId,
                    Status = WalletTransactionStatus.Success.ToString(),
                    Target = paymentRequest.ShopId.ToString(),
                    Type = WalletTransactionType.Commission.ToString(),
                    WalletId = wallet.Id,
                };
                await _walletTransactionRepository.InsertAsync(transaction);
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
        public async Task<bool> AddFundsAsync(Guid walletId, decimal amount, string modifiedBy)
        {
            try
            {
                var wallet = await _walletRepository.GetByIdAsync(walletId.ToString());
                if (wallet == null)
                {
                    _logger.LogWarning("Không tìm thấy ví {WalletId} để thêm tiền", walletId);
                    return false;
                }

                wallet.Balance += amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                wallet.SetModifier(modifiedBy);
                await _walletRepository.ReplaceAsync(walletId.ToString(), wallet);

                _logger.LogInformation("Đã cập nhật balance cho ví {WalletId}, số tiền thêm: {Amount}", walletId, amount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm tiền vào ví {WalletId}", walletId);
                return false;
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