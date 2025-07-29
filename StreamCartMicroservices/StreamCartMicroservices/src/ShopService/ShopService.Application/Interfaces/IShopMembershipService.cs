using Shared.Common.Models;
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

    }
}
