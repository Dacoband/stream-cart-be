using Microsoft.EntityFrameworkCore;
using Shared.Common.Data.Repositories;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Domain.Enums;
using ShopService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Repositories
{
    public class ShopMembershipRepository : EfCoreGenericRepository<ShopMembership>, IShopMembershipRepository
    {
        private readonly ShopContext _context;

        public ShopMembershipRepository(ShopContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ShopMembership?> GetActiveMembership(string shopId)
        {
            return await _context.ShopMembership.Include(x => x.Membership).Where(x => x.ShopID.ToString() == shopId && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.Membership.Type == "New" && x.IsDeleted==false && x.Status != "Canceled").FirstOrDefaultAsync();
        }
    }
}
