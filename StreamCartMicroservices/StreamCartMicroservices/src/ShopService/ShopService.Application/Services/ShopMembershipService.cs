using Appwrite;
using Appwrite.Models;
using Appwrite.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Extensions;
using Shared.Common.Models;
using ShopService.Application.DTOs.Membership;
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
    public class ShopMembershipService : IShopMembershipService
    {
        private readonly IShopMembershipRepository _shopMembershipRepository;
        private readonly IShopRepository _shopRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IWalletRepository _walletRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IShopUnitOfWork _shopUnitOfWork;
        public ShopMembershipService(IShopMembershipRepository shopMembershipRepository, IShopRepository shopRepository, IAccountServiceClient accountServiceClient, IWalletRepository walletRepository, IMembershipRepository membershipRepository, IWalletTransactionRepository walletTransactionRepository, IShopUnitOfWork shopUnitOfWork)

        {
            _accountServiceClient = accountServiceClient;
            _shopMembershipRepository = shopMembershipRepository;
            _shopRepository = shopRepository;
            _walletRepository = walletRepository;
            _membershipRepository = membershipRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _shopUnitOfWork = shopUnitOfWork;
        }
        public async Task<ApiResponse<ShopMembership>> CreateShopMembership(string membershipId, string userId)
        {
            var result = new ApiResponse<ShopMembership>
            {
                Success = true,
                Message = "Mua gói thành viên thành công"
            };

            try
            {
                var now = DateTime.UtcNow;

                // Lấy thông tin người dùng và cửa hàng
                var user = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(userId));
                var shop = await _shopRepository.GetByIdAsync(user.ShopId);
                if (shop == null)
                    return Fail("Không tìm thấy thông tin cửa hàng");
                if (shop.IsDeleted || shop.Status != Domain.Enums.ShopStatus.Active || shop.ApprovalStatus != ApprovalStatus.Approved)
                    return Fail("Cửa hàng không đủ điều kiện để hoạt động");

                // Lấy thông tin ví
                var wallet = await _walletRepository.GetByShopIdAsync(shop.Id);
                if (wallet == null || wallet.IsDeleted)
                    return Fail("Không tìm thấy thông tin ví của cửa hàng");

                // Lấy thông tin gói thành viên
                var membership = await _membershipRepository.GetByIdAsync(membershipId);
                if (membership == null || membership.IsDeleted)
                    return Fail("Không tìm thấy thông tin gói thành viên");

                // Kiểm tra số dư
                if (wallet.Balance < membership.Price)
                    return Fail("Số tiền trong ví không đủ để mua gói thành viên");

                // Xử lý thời gian & trạng thái gói
                var existingShopMembership = await _shopMembershipRepository.GetActiveMembership(shop.Id.ToString());

                DateTime startDate, endDate;
                string status;
                startDate = DateTime.SpecifyKind(now, DateTimeKind.Unspecified);
                status = "Ongoing";
                // Thay thế toàn bộ phần xử lý thời gian bằng đoạn đã sửa dưới đây:
                if (membership.Type == "New")
                {
                    if (existingShopMembership != null)
                    {                        
                        //xóa gói hiện tại
                        existingShopMembership.Status = "Overdue";
                        existingShopMembership.EndDate = DateTime.SpecifyKind(now, DateTimeKind.Unspecified);
                        await _shopMembershipRepository.ReplaceAsync(existingShopMembership.Id.ToString(), existingShopMembership);
                    }
                   
                   
                }
                else if (membership.Type == "Renewal")
                {
                    if (existingShopMembership == null)
                        return Fail("Không thể gia hạn vì cửa hàng chưa có gói thành viên hiện tại");

                    startDate = DateTime.SpecifyKind(now, DateTimeKind.Unspecified);
                  
                    status = "Ongoing";
                }
                else
                {
                    return Fail("Loại gói thành viên không hợp lệ");
                }

                // Tạo bản ghi gói thành viên cửa hàng
                var shopMembership = new ShopMembership
                {
                    MembershipID = Guid.Parse(membershipId),
                    ShopID = shop.Id,
                    StartDate = startDate,
                    Status = status,
                    RemainingLivestream = membership.MaxLivestream,
                    Commission = membership.Commission,
                    MaxProduct = membership.MaxProduct,
                    MaxLivestream = membership.MaxLivestream,
                };
                shopMembership.SetCreator(userId);
                shopMembership.SetModifier(userId);  
                // Trừ tiền ví
                wallet.Balance -= membership.Price;
                wallet.SetModifier(userId);

                // Ghi nhận giao dịch
                var transaction = new WalletTransaction
                {
                    Type = "System",
                    Amount = membership.Price * -1,
                    Target = "System",
                    Status = "Success",
                    WalletId = wallet.Id,
                    ShopMembershipId = membership.Id,
                    BankAccount = wallet.BankName,
                    BankNumber = wallet.BankAccountNumber,
                };
                transaction.SetCreator(userId);

                // Bắt đầu transaction
                await _shopUnitOfWork.BeginTransactionAsync();

                await _shopMembershipRepository.InsertAsync(shopMembership);
                await _walletRepository.ReplaceAsync(wallet.Id.ToString(), wallet);
                await _walletTransactionRepository.InsertAsync(transaction);

                await _shopUnitOfWork.CommitTransactionAsync();

                result.Data = shopMembership;
                return result;
            }
            catch (Exception ex)
            {
                await _shopUnitOfWork.RollbackTransactionAsync();

                return new ApiResponse<ShopMembership>
                {
                    Success = false,
                    Message = "Lỗi khi mua gói thành viên: " + ex.Message
                };
            }

            // Helper
            ApiResponse<ShopMembership> Fail(string message) => new()
            {
                Success = false,
                Message = message
            };
        }

        public async Task<ApiResponse<DetailShopMembershipDTO>> DeleteShopMembership(string id, string userId)
        {
            var result = new ApiResponse<DetailShopMembershipDTO>
            {
                Message = "Xóa gói thành viên thành công",
                Success = true,
            };
            var existingShopMembership = await _shopMembershipRepository.GetByIdAsync(id);
            if (existingShopMembership == null || existingShopMembership.IsDeleted == true) {
                result.Success = false;
                result.Message = "Không tìm thấy gói thành viên";
                return result;
            };
            if(existingShopMembership.Status == "Overdue" || existingShopMembership.Status == "Canceled")
            {
                result.Success = false;
                result.Message = "Không thể xóa gói thành viên đã quá hạn";
                return result;
            }
            if (existingShopMembership.CreatedBy != userId) {
                result.Success = false;
                result.Message = "Bạn không có quyền hủy gói thành viên này";
                return result;
            }
            existingShopMembership.Delete(userId);
            existingShopMembership.Status = "Canceled";
            try
            {
                await _shopMembershipRepository.ReplaceAsync(id, existingShopMembership);
                 var createdBy = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(existingShopMembership.CreatedBy));
                var modifiedBy = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(existingShopMembership.LastModifiedBy));
                result.Data = new DetailShopMembershipDTO()
                {
                    Id = existingShopMembership.Id.ToString(),
                    ShopID = existingShopMembership.ShopID,
                    StartDate = existingShopMembership.StartDate,
                    EndDate = (DateTime)existingShopMembership.EndDate,
                    RemainingLivestream = existingShopMembership.RemainingLivestream,
                    Status = existingShopMembership.Status,
                    CreatedBy = createdBy.Fullname ?? "N/A",
                    CreatedAt = existingShopMembership.CreatedAt,
                    ModifiedBy = modifiedBy.Fullname ?? "N/A",
                    ModifiedAt = existingShopMembership.LastModifiedAt,
                    IsDeleted = existingShopMembership.IsDeleted,
                    MaxProduct = existingShopMembership.MaxProduct,
                    Commission = existingShopMembership.Commission,

                };
                return result;
            }catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }


        }

        public async Task<ApiResponse<ListShopMembershipDTO>> FilterShopMembership(FilterShopMembership filter)
        {
            var result = new ApiResponse<ListShopMembershipDTO>
            {
                Message = "Lọc danh sách gói thành viên thành công",
                Success = true,
            };

            try
            {
                // Bắt đầu từ toàn bộ danh sách
                var shopmembershipList = await _shopMembershipRepository.GetAll();
                var query = shopmembershipList.AsQueryable();
                // Áp dụng các bộ lọc nếu có
                if (!string.IsNullOrWhiteSpace(filter.ShopId))
                    query = query.Where(x => x.ShopID.ToString() == filter.ShopId);

                if (!string.IsNullOrWhiteSpace(filter.MembershipType))
                    query = query.Where(x => x.Membership.Type == filter.MembershipType);

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    query = query.Where(x => x.Status == filter.Status);

                if (filter.StartDate.HasValue)
                    query = query.Where(x => x.StartDate >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    query = query.Where(x => x.EndDate <= filter.EndDate.Value);

                // Paging
                int pageIndex = filter.PageIndex ?? 1;
                int pageSize = filter.PageSize ?? 10;
                shopmembershipList = query.OrderByDescending(x => x.CreatedAt)
                             .Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize).ToList();

                var totalItems =  query.Count();

                

             

                result.Data = new ListShopMembershipDTO
                {
                    TotalItem = totalItems,
                    DetailShopMembership = shopmembershipList.Select(x => new DetailShopMembershipDTO
                    {
                        Id = x.Id.ToString(),
                        ShopID = x.ShopID,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        RemainingLivestream = x.RemainingLivestream,
                        Status = x.Status,
                        CreatedAt = x.CreatedAt,
                        ModifiedAt = x.LastModifiedAt,
                        IsDeleted = x.IsDeleted,
                        MaxProduct = x.MaxProduct,
                        Commission = x.Commission,
                        CreatedBy = x.CreatedBy,
                    }).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
        }

        public async Task<ApiResponse<DetailShopMembershipDTO>> GetShopMembershipById(string id)
        {
            var result = new ApiResponse<DetailShopMembershipDTO>()
            {
                Message = "Tìm gói thành viên thành công",
                Success = true,
            };
            try
            {
                var existingShopMembership = await _shopMembershipRepository.GetById(id);
                if (existingShopMembership == null)
                {
                    result.Success = false;
                    result.Message = "Không tìm thấy gói thành viên của cửa hàng";
                    return result;

                }
                var createdBy = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(existingShopMembership.CreatedBy));
                var modifiedByName = "N/A";
                if (!existingShopMembership.LastModifiedBy.IsNullOrEmpty())
                {
                   var  modifiedBy = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(existingShopMembership.LastModifiedBy));
                    modifiedByName = modifiedBy.Fullname;
                }
                result.Data = new DetailShopMembershipDTO()
                {
                    Id = existingShopMembership.Id.ToString(),
                    ShopID = existingShopMembership.ShopID,
                    StartDate = existingShopMembership.StartDate,
                    EndDate = (DateTime)existingShopMembership.EndDate,
                    RemainingLivestream = existingShopMembership.RemainingLivestream,
                    Status = existingShopMembership.Status,
                    CreatedBy = createdBy.Fullname ?? "N/A",
                    CreatedAt = existingShopMembership.CreatedAt,
                    ModifiedBy = modifiedByName,
                    ModifiedAt = existingShopMembership.LastModifiedAt,
                    IsDeleted = existingShopMembership.IsDeleted,
                    MaxProduct = existingShopMembership.MaxProduct,
                    Commission = existingShopMembership.Commission,
                };
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;

            }
        }

        public async Task<ApiResponse<DetailShopMembershipDTO>> UpdateShopMembership(string shopId, int remaingLivstream)
        {
            var result = new ApiResponse<DetailShopMembershipDTO>
            {
                Message = "Cập nhật gói thành viên thành công",
                Success = true,
            };
            var existingShopMembership = await _shopMembershipRepository.GetByIdAsync(shopId);
            if (existingShopMembership == null || existingShopMembership.IsDeleted == true)
            {
                result.Success = false;
                result.Message = "Không tìm thấy gói thành viên";
                return result;
            };
            if (existingShopMembership.Status == "Overdue" || existingShopMembership.Status == "Canceled")
            {
                result.Success = false;
                result.Message = "Không thể cập nhật gói thành viên đã quá hạn";
                return result;
            }
            existingShopMembership.RemainingLivestream = remaingLivstream;
            existingShopMembership.SetModifier("system");

            try
            {
                await _shopMembershipRepository.ReplaceAsync(existingShopMembership.Id.ToString(), existingShopMembership);
                var createdBy = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(existingShopMembership.CreatedBy));
                var modifiedBy = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(existingShopMembership.LastModifiedBy));
                result.Data = new DetailShopMembershipDTO()
                {
                    Id = existingShopMembership.Id.ToString(),
                    ShopID = existingShopMembership.ShopID,
                    StartDate = existingShopMembership.StartDate,
                    EndDate = (DateTime)existingShopMembership.EndDate,
                    RemainingLivestream = existingShopMembership.RemainingLivestream,
                    Status = existingShopMembership.Status,
                    CreatedBy = createdBy.Fullname ?? "N/A",
                    CreatedAt = existingShopMembership.CreatedAt,
                    ModifiedBy = modifiedBy.Fullname ?? "N/A",
                    ModifiedAt = existingShopMembership.LastModifiedAt,
                    IsDeleted = existingShopMembership.IsDeleted,
                    MaxProduct = existingShopMembership.MaxProduct,
                    Commission = existingShopMembership.Commission,

                };
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
        }
        /// <summary>
        /// Trừ phút livestream theo thứ tự: gói NEW trước, thiếu thì trừ tiếp gói RENEWAL.
        /// Trả về tổng phút còn lại sau khi trừ.
        /// </summary>
        /// <summary>
        /// Trừ số phút livestream: gói NEW trước, nếu không đủ thì trừ tiếp gói RENEWAL.
        /// </summary>
        public async Task<ApiResponse<int>> UpdateLivestreamRemaining(int usedMinutes, string shopId)
        {
            var result = new ApiResponse<int>
            {
                Message = "Cập nhật số phút livestream còn lại thành công",
                Success = true,
                Data = 0
            };

            if (string.IsNullOrWhiteSpace(shopId))
                return Fail(result, "Thiếu shopId");

            if (usedMinutes <= 0)
            {
                result.Data = await GetRemainingLivestream(shopId);
                return result;
            }

            var activeMemberships = await GetActiveMembershipsAsync(shopId);
            if (activeMemberships.Count == 0)
                return Fail(result, "Shop không có gói thành viên đang hoạt động");

            // Ưu tiên: NEW trước, rồi RENEWAL
            activeMemberships = activeMemberships
                .OrderBy(m => IsNew(m.Membership) ? 0 : 1)
                .ThenBy(m => m.StartDate)
                .ToList();

            var toDeduct = usedMinutes;

            foreach (var m in activeMemberships)
            {
                if (toDeduct <= 0) break;

                var remaining = GetRemainingMinutes(m);
                if (remaining <= 0) continue;

                var deduct = Math.Min(remaining, toDeduct);
                m.RemainingLivestream = Math.Max(0, remaining - deduct);

                await _shopMembershipRepository.ReplaceAsync(m.Id.ToString(), m);

                toDeduct -= deduct;
            }

            result.Data = await GetRemainingLivestream(shopId);

            if (toDeduct > 0)
                result.Message = "Đã trừ hết số phút hiện có. Số phút yêu cầu vượt quá tổng còn lại.";

            return result;
        }

        /// <summary>
        /// Lấy tổng số phút livestream còn lại của shop.
        /// </summary>
        public async Task<int> GetRemainingLivestream(string shopId)
        {
            if (string.IsNullOrWhiteSpace(shopId)) return 0;

            var activeMemberships = await GetActiveMembershipsAsync(shopId);
            if (activeMemberships.Count == 0) return 0;

            return activeMemberships.Sum(GetRemainingMinutes);
        }

        // ======================
        //  Private helpers
        // ======================

        private async Task<List<ShopMembership>> GetActiveMembershipsAsync(string shopId)
        {
            var memberships = await _shopMembershipRepository.GetAllAciveShopMembership(shopId);
            return memberships ?? new List<ShopMembership>();
        }

        private static bool IsNew(Domain.Entities.Membership m)
        {
            var type = m?.Type?.ToString() ?? string.Empty;
            return type.Equals("new", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetRemainingMinutes(ShopMembership m)
            => Math.Max(0, m?.RemainingLivestream ?? 0);

        private static ApiResponse<int> Fail(ApiResponse<int> r, string message)
        {
            r.Success = false;
            r.Message = message;
            return r;
        }
    }
}
