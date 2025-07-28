using Microsoft.EntityFrameworkCore;
using Shared.Common.Models;
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

            if (existingMembership.Type == "New")
            {
                startDate = existingShopMembership == null ? now : existingShopMembership.EndDate;
                endDate = startDate.AddMonths(existingMembership.Duration);
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
            }

            var shopMembership = new ShopMembership
            {
                MembershipID = Guid.Parse(membershipId),
                ShopID = existingShop.Id,
                StartDate = startDate,
                EndDate = endDate,
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
    }
}
