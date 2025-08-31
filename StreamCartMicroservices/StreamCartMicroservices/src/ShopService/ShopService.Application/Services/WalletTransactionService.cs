using Appwrite.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Extensions;
using Shared.Common.Models;
using ShopService.Application.DTOs.WalletTransaction;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Services
{
    public class WalletTransactionService : IWalletTransactionService
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IWalletRepository _walletRepository;
        public WalletTransactionService(IWalletTransactionRepository walletTransactionRepository, IWalletRepository walletRepository)
        {
            _walletTransactionRepository = walletTransactionRepository;
            _walletRepository = walletRepository;
        }
        public async Task<ApiResponse<WalletTransaction>> CreateWalletTransaction(
    CreateWalletTransactionDTO request,
    string? shopId,
    string userId)
        {
            var response = new ApiResponse<WalletTransaction>
            {
                Success = true,
                Message = "Tạo giao dịch ví thành công"
            };

            // ===== 1) Validate input =====
            if (!Guid.TryParse(shopId, out var shopGuid))
            {
                response.Success = false;
                response.Message = "shopId không hợp lệ";
                return response;
            }

            if (request.Amount <= 0)
            {
                response.Success = false;
                response.Message = "Số tiền giao dịch phải lớn hơn 0";
                return response;
            }

            if (request.Type == WalletTransactionType.Withdraw && request.Amount <= 50_000)
            {
                response.Success = false;
                response.Message = "Số tiền rút về phải lớn hơn 50.000";
                return response;
            }

            var wallet = await _walletRepository.GetByShopIdAsync(shopGuid);
            if (wallet == null)
            {
                response.Success = false;
                response.Message = "Không tìm thấy thông tin ví";
                return response;
            }

            // ===== 2) Xác định dòng tiền và kiểm tra số dư =====
            // Tiền vào ví (deposit, refund về ví, ...): dương
            // Tiền ra ví (withdraw, system trừ, ...): âm
            var isOutflow = request.Type == WalletTransactionType.Withdraw
                            || request.Type == WalletTransactionType.System;

            decimal signedAmount = isOutflow ? -request.Amount : request.Amount;

            if (isOutflow && wallet.Balance < request.Amount)
            {
                response.Success = false;
                response.Message = "Số tiền trong ví không đủ để thực hiện giao dịch";
                return response;
            }

            // ===== 3) Khởi tạo transaction =====
            var walletTransaction = new WalletTransaction
            {
                Type = request.Type.ToString(),
                Amount = signedAmount,
                Status = request.Status.ToString(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
                WalletId = wallet.Id,
                // Chuẩn hóa tên ngân hàng: "MBBank" / "MB Bank" -> "MB"
                BankAccount = NormalizeBankName(wallet.BankName),
                BankNumber = wallet.BankAccountNumber,
                TransactionId = string.IsNullOrWhiteSpace(request.TransactionId) ? null : request.TransactionId,
                Target = isOutflow ? Guid.Empty.ToString() : null
            };

            if (request.ShopMembershipId.HasValue)
                walletTransaction.ShopMembershipId = request.ShopMembershipId;

            if (request.OrderId.HasValue)
                walletTransaction.OrderId = request.OrderId;

            if (request.RefundId.HasValue)
                walletTransaction.RefundId = request.RefundId;

            // Build mô tả nếu không truyền
            if (string.IsNullOrWhiteSpace(walletTransaction.Description))
            {
                var absAmount = Math.Abs(signedAmount);
                walletTransaction.Description = BuildVnDescription(
                    request.Type,
                    absAmount,                     // mô tả nên dùng số dương
                    wallet.BankName,
                    wallet.BankAccountNumber,
                    walletTransaction.TransactionId,
                    walletTransaction.OrderId,
                    walletTransaction.RefundId,
                    walletTransaction.ShopMembershipId
                );
            }

            walletTransaction.SetCreator(userId);

            // ===== 4) Cập nhật số dư khi giao dịch thành công =====
            // Quy ước: chỉ khi Status = Success mới ghi vào số dư
            if (request.Status == WalletTransactionStatus.Success)
            {
                wallet.Balance += signedAmount; // deposit tăng, withdraw/system giảm
                wallet.SetModifier("system");
            }

            // ===== 5) Lưu DB =====
            try
            {
                await _walletTransactionRepository.InsertAsync(walletTransaction);
                await _walletRepository.ReplaceAsync(wallet.Id.ToString(), wallet);

                response.Data = walletTransaction;
                return response;
            }
            catch (Exception)
            {
                response.Success = false;
                response.Message = "Lỗi khi tạo giao dịch ví";
                return response;
            }
        }
        private static string NormalizeBankName(string bankName)
        {
            if (string.IsNullOrWhiteSpace(bankName)) return string.Empty;

            var normalizedNoSpace = bankName.Replace(" ", "").Trim().ToLower();

            // Mapping gọn (bỏ khoảng trắng so khớp)
            return normalizedNoSpace switch
            {
                "mbbank" => "MB",
                "mbbanknganhangquanda" => "MB", // nếu có biến thể hiếm
                _ => bankName.Trim().Equals("MB Bank", StringComparison.OrdinalIgnoreCase) ? "MB" : bankName.Trim()
            };
        }
        private static string BuildVnDescription(
    WalletTransactionType type,
    decimal normalizedAmount,
    string bankName,
    string bankNumber,
    string? transactionId,
    Guid? orderId,
    Guid? refundId,
    Guid? shopMembershipId)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
            string Money(decimal v) => $"{Math.Abs(v).ToString("N0", culture)}đ";

            string suffixTranId = string.IsNullOrWhiteSpace(transactionId) ? "" : $" (Mã GD: {transactionId})";

            switch (type)
            {
                case WalletTransactionType.Deposit:
                    return $"Nạp {Money(normalizedAmount)} vào ví{suffixTranId}.";

                case WalletTransactionType.Withdraw:
                    return $"Yêu cầu rút {Money(normalizedAmount)} về ngân hàng {bankName} - {bankNumber}{suffixTranId}.";

                case WalletTransactionType.Commission:
                    if (orderId.HasValue)
                        return $"Thanh toán đơn hàng #{orderId.Value.ToString()[..8]} số tiền {Money(normalizedAmount)}.";
                    return $"Thanh toán đơn hàng số tiền {Money(normalizedAmount)}.";

                

                case WalletTransactionType.System:
                    if (shopMembershipId.HasValue)
                        return $"Thanh toán gói thành viên #{shopMembershipId.Value.ToString()[..8]} số tiền {Money(normalizedAmount)}.";
                    return $"Thanh toán gói thành viên số tiền {Money(normalizedAmount)}.";

                

                default:
                    // Dự phòng
                    return $"Giao dịch {type}: {Money(normalizedAmount)}{suffixTranId}.";
            }
        }
        public async Task<ApiResponse<WalletTransaction>> GetWalletTransactionById(string id)
        {
            var response = new ApiResponse<WalletTransaction>
            {
                Success = true,
                Message = "Lấy giao dịch ví thành công"
            };

            if (string.IsNullOrWhiteSpace(id))
            {
                return ApiResponse<WalletTransaction>.ErrorResult("Id giao dịch không hợp lệ");
            }

            if (!Guid.TryParse(id, out var transactionId))
            {
                return ApiResponse<WalletTransaction>.ErrorResult("Id giao dịch không đúng định dạng");
            }

            try
            {
                var transaction = await _walletTransactionRepository.GetByIdAsync(id);
                if (transaction == null)
                {
                    return ApiResponse<WalletTransaction>.ErrorResult("Không tìm thấy giao dịch ví");
                }

                response.Data = transaction;
                return response;
            }
            catch (Exception)
            {
                return ApiResponse<WalletTransaction>.ErrorResult("Lỗi khi lấy giao dịch ví");
            }
        }

        public async Task<ApiResponse<ListWalletransationDTO>> GetWalletTransactionList(FilterWalletTransactionDTO filter, string? shopId)
        {
            var all = await _walletTransactionRepository.GetAllAsync();
            var query = all.AsQueryable();

            filter ??= new FilterWalletTransactionDTO();
            if (!string.IsNullOrWhiteSpace(shopId))
            {
                filter.ShopId = shopId;
            }
            // 1) Lọc theo Types (enum) -> cột string
            if (filter.Types is { Count: > 0 })
            {
                var typeStrings = filter.Types.Select(t => t.ToString()).ToList();
                query = query.Where(x => typeStrings.Contains(x.Type));
            }

            // 2) Lọc theo Status (enum) -> cột string
            if (filter.Status is { Count: > 0 })
            {
                var statusStrings = filter.Status.Select(s => s.ToString()).ToList();
                query = query.Where(x => statusStrings.Contains(x.Status));
            }

            // 3) Lọc theo Target
            if (!string.IsNullOrWhiteSpace(filter.Target))
            {
                query = query.Where(x => x.Target == filter.Target);
            }

            // 4) Lọc theo ShopId
            if (!string.IsNullOrWhiteSpace(filter.ShopId) && Guid.TryParse(filter.ShopId, out var sId))
            {
                var wallet = await _walletRepository.GetByShopIdAsync(sId);
                if (wallet != null)
                {
                    query = query.Where(x => x.WalletId == wallet.Id);
                }
            }

            // 5) Lọc theo thời gian
            DateTime? from = filter.FromTime;
            DateTime? to = filter.ToTime;

            if (from.HasValue && to.HasValue && from.Value > to.Value)
                (from, to) = (to, from);

            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
                query = query.Where(x => x.CreatedAt >= fromUtc);
            }

            if (to.HasValue)
            {
                var toInclusive = to.Value.Kind == DateTimeKind.Unspecified
                    ? to.Value.Date.AddDays(1).AddTicks(-1)
                    : to.Value;

                var toUtc = DateTime.SpecifyKind(toInclusive, DateTimeKind.Utc);
                query = query.Where(x => x.CreatedAt <= toUtc);
            }

            // 6) Sắp xếp & phân trang
            query = query.OrderByDescending(x => x.CreatedAt);

            const int MAX_PAGE_SIZE = 200;
            int pageIndex = filter.PageIndex <= 0 ? 1 : filter.PageIndex;
            int pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, MAX_PAGE_SIZE);

            int totalCount = query.Count();
            int totalPage = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = query.Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            var result = new ListWalletransationDTO
            {
                Items = items,
                TotalCount = totalCount,
                TotalPage = totalPage
            };

            var message = $"Lấy {items.Count} giao dịch (trang {pageIndex}/{totalPage}, kích thước {pageSize}) / tổng {totalCount}.";
            return ApiResponse<ListWalletransationDTO>.SuccessResult(result, message);
        }

        public async Task<ApiResponse<WalletTransaction>> UpdateWalletTransactionStatus(
    string id,
    WalletTransactionStatus status,
    string? shopid, string userid)
        {
            var tx = await _walletTransactionRepository.GetByIdAsync(id);
            if (tx == null)
                return ApiResponse<WalletTransaction>.ErrorResult("Không tìm thấy giao dịch");

            var wallet = await _walletRepository.GetByIdAsync(tx.WalletId.ToString());

            if (shopid != null && wallet.ShopId != Guid.Parse(shopid))
                return ApiResponse<WalletTransaction>.ErrorResult("Bạn không có quyền cập nhật giao dịch của shop khác");

            // Kiểm tra trạng thái hiện tại
            if (!Enum.TryParse<WalletTransactionStatus>(tx.Status, out var currentStatus))
                return ApiResponse<WalletTransaction>.ErrorResult("Trạng thái hiện tại của giao dịch không hợp lệ");

            if (currentStatus != WalletTransactionStatus.Pending)
                return ApiResponse<WalletTransaction>.ErrorResult("Chỉ giao dịch ở trạng thái Pending mới được cập nhật");

            if (status == WalletTransactionStatus.Pending)
                return ApiResponse<WalletTransaction>.ErrorResult("Không thể cập nhật giao dịch sang Pending");

            // Thực hiện cập nhật trạng thái
            if(status == WalletTransactionStatus.Success && tx.Type == WalletTransactionType.Withdraw.ToString())
            {
               wallet.Balance = wallet.Balance + tx.Amount;
               wallet.SetModifier("system");
            }
            tx.Status = status.ToString();
            tx.SetModifier(userid);
            try
            {
                await _walletTransactionRepository.ReplaceAsync(tx.Id.ToString(),tx);
                await _walletRepository.ReplaceAsync(wallet.Id.ToString(),wallet);
                return ApiResponse<WalletTransaction>.SuccessResult(tx, $"Cập nhật trạng thái giao dịch thành {status} thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<WalletTransaction>.ErrorResult($"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }

    }
}
