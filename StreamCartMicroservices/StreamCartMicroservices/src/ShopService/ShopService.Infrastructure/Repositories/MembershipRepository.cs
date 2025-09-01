
using Microsoft.EntityFrameworkCore;
using Shared.Common.Data.Repositories;
using ShopService.Application.Interfaces;
using ShopService.Domain.Entities;
using ShopService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Infrastructure.Repositories
{
    public class MembershipRepository : EfCoreGenericRepository<Membership>, IMembershipRepository
    {
        private readonly ShopContext _context;

        public MembershipRepository(ShopContext context) : base(context)
        {
            _context = context;
        }
        public new async Task<Membership?> GetById(string id)
        {
            if (Guid.TryParse(id, out Guid guidId))
            {
                return await _context.Membership
                     .IgnoreQueryFilters() 
                    .FirstOrDefaultAsync(m => m.Id == guidId);
            }
            return null;
        }
        public async Task<Membership?> GetActiveByIdAsync(string id)
        {
            if (Guid.TryParse(id, out Guid guidId))
            {
                return await _context.Membership
                    .FirstOrDefaultAsync(m => m.Id == guidId && !m.IsDeleted);
            }
            return null;
        }

        public new async Task<IEnumerable<Membership>> GetAll()
        {
            return await _context.Membership.IgnoreQueryFilters() 
                .ToListAsync();
        }
        public async Task<IEnumerable<Membership>> GetAllActiveAsync()
        {
            return await _context.Membership
                .Where(m => !m.IsDeleted)
                .ToListAsync();
        }
    }
}
