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
    public interface IMembershipService
    {
        public Task<ApiResponse<DetailMembershipDTO>> CreateMembership(CreateMembershipDTO request,string userId);
        public Task<ApiResponse<DetailMembershipDTO>> UpdateMembership(UpdateMembershipDTO request, string userId, string membershipIds);
        public Task<ApiResponse<DetailMembershipDTO>> DeleteMembership(string request, string userId);
        public Task<ApiResponse<DetailMembershipDTO>> GetMembershipById(string id);
        public Task<ApiResponse<ListMembershipDTO>> FilterMembership(FilterMembershipDTO filter);
    }
}
