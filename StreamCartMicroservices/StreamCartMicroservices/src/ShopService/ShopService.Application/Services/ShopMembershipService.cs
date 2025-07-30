using Appwrite;
using Appwrite.Models;
using Appwrite.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
            var result = new ApiResponse<ShopMembership>()
            {
                Message = "Mua gói thành viên thành công",
                Success = true,
            };
            //check seller
            var user = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(userId));
            var existingShop =await _shopRepository.GetByIdAsync(user.ShopId);
            //check validated shop
            if (existingShop == null) { 
            result.Success = false;
                result.Message = "Không tìm thấy thông tin cửa hàng";
                return result;
            
            }

            if(existingShop.IsDeleted == true || existingShop.Status !=  Domain.Enums.ShopStatus.Active ||existingShop.ApprovalStatus != ApprovalStatus.Approved)
            {
                result.Success = false;
                result.Message = "Cửa hàng không đủ điều kiện để hoạt động";
                return result;
            }
            //check validated wallet
            var existingWallet = await _walletRepository.GetByShopIdAsync(existingShop.Id);
            if (existingWallet == null || existingWallet.IsDeleted == true) { 
                result.Success= false;
                result.Message = "Không tìm thấy thông tin ví của cửa hàng";
                return result;
            }
            //check existing membership
            var existingMembership = await _membershipRepository.GetByIdAsync(membershipId);
            if (existingMembership == null || existingMembership.IsDeleted == true) {
                result.Success = false;
                result.Message = "Không tìm thấy thông tin gói thành viên";
                return result;
            }
            //check wallet ballance
            if(existingWallet.Balance <= existingMembership.Price)
            {
                result.Success = false;
                result.Message = "Số tiền trong ví không đủ để mua gói thành viên";
                return result;
            }
            //check existing shopmembership
            var existingShopMembership = await _shopMembershipRepository.GetActiveMembership(existingShop.Id.ToString());

            var now = DateTime.UtcNow;
            DateTime startDate  = DateTime.UtcNow;
            DateTime endDate = DateTime.UtcNow;
            string status = "";
            if (existingMembership.Type == "New")
            {
                if(existingMembership == null)
                {
                    startDate = now;
                    status = "Ongoing";
                }
                else
                {
                    startDate = existingShopMembership.EndDate;
                    status = "Waiting";
                }
               
            }
            else if (existingMembership.Type == "Renewal")
            {
                if (existingShopMembership == null)
                {
                    result.Success = false;
                    result.Message = "Không thể gia hạn vì cửa hàng chưa có gói thành viên hiện tại";
                    return result;
                }

                startDate = now;
                endDate = existingShopMembership.EndDate;
                status = "Ongoing";
            }

            var shopMembership = new ShopMembership
            {
                MembershipID = Guid.Parse(membershipId),
                ShopID = existingShop.Id,
                StartDate = startDate,
                EndDate = endDate,
                RemainingLivestream = existingMembership.MaxLivestream,
                Commission = existingMembership.Commission,
                MaxProduct = existingMembership.MaxProduct,
                
            };
            shopMembership.SetCreator(userId);
            existingWallet.Balance = existingWallet.Balance - existingMembership.Price;
            existingWallet.SetModifier(userId);
            var transaction = new WalletTransaction()
            {
                Type = "Membership_Purchase",
                Amount = existingMembership.Price,
                Target = "System",
                Status = "Success",
                WalletId = existingWallet.Id,
                ShopMembershipId = existingMembership.Id,

            };
            transaction.SetCreator(userId);

            try
            {
                await _shopUnitOfWork.BeginTransactionAsync();

                await _shopMembershipRepository.InsertAsync(shopMembership);
                await _walletRepository.ReplaceAsync(existingWallet.Id.ToString(), existingWallet);
                await _walletTransactionRepository.InsertAsync(transaction);
                result.Data = shopMembership;
                await _shopUnitOfWork.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex) {
                await _shopUnitOfWork.RollbackTransactionAsync();

                result.Success = false;
                result.Message = ex.Message;
                return result;
            
            }
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
                    EndDate = existingShopMembership.EndDate,
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
                var query = await _shopMembershipRepository.GetAllAsync(); // Giả sử bạn có IQueryable
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
                query = query.OrderByDescending(x => x.CreatedAt)
                             .Skip((pageIndex - 1) * pageSize)
                             .Take(pageSize).ToList();

                var totalItems =  query.Count();

                

             

                result.Data = new ListShopMembershipDTO
                {
                    TotalItem = totalItems,
                    DetailShopMembership = query.Select(x => new DetailShopMembershipDTO
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
                var existingShopMembership = await _shopMembershipRepository.GetByIdAsync(id);
                if (existingShopMembership == null)
                {
                    result.Success = false;
                    result.Message = "Không tìm thấy gói thành viên của cửa hàng";
                    return result;

                }
                var createdBy = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(existingShopMembership.CreatedBy));
                var modifiedBy = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(existingShopMembership.LastModifiedBy));
                result.Data = new DetailShopMembershipDTO()
                {
                    Id = existingShopMembership.Id.ToString(),
                    ShopID = existingShopMembership.ShopID,
                    StartDate = existingShopMembership.StartDate,
                    EndDate = existingShopMembership.EndDate,
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
                    EndDate = existingShopMembership.EndDate,
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
    }
}
