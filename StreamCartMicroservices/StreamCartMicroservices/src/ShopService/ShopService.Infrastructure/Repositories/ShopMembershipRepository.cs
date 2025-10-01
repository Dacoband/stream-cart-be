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
            return await _context.ShopMembership.Include(x => x.Membership).Where(x => x.ShopID.ToString() == shopId && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.Membership.Type == "New" && x.IsDeleted == false && x.Status == "Ongoing").FirstOrDefaultAsync();
        }
        public async Task<ShopMembership?> GetById(string id)
        {
            return await _context.ShopMembership.Where(x => x.Id.ToString() == id).FirstOrDefaultAsync();
        }
        public async Task<List<ShopMembership>> GetAll()
        {
            return await _context.ShopMembership.ToListAsync();
        }

        public async Task<List<ShopMembership>> GetAllAciveShopMembership(string shopId)
        {
            return await _context.ShopMembership.Include(x => x.Membership).Where(x => x.ShopID.ToString() == shopId && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now && x.IsDeleted == false && x.Status != "Ongoing" && x.RemainingLivestream >0).ToListAsync();
        }
    }
}
