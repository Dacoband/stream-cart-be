using Shared.Common.Models;
using ShopService.Application.DTOs.Membership;
using ShopService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Interfaces
{
    public interface IShopMembershipService
    {
        public Task<ApiResponse<ShopMembership>> CreateShopMembership(string membershipId, string userId);
        public Task<ApiResponse<DetailShopMembershipDTO>> GetShopMembershipById(string id);
        public Task<ApiResponse<ListShopMembershipDTO>> FilterShopMembership(FilterShopMembership filter);
        public Task<ApiResponse<DetailShopMembershipDTO>> DeleteShopMembership(string id, string userId);
        public Task<ApiResponse<DetailShopMembershipDTO>> UpdateShopMembership(string shopId, int remaingLivstream);

    }
}
