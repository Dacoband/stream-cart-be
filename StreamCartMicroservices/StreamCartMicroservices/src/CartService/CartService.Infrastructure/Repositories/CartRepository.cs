using CartService.Domain.Entities;
using CartService.Infrastructure.Data;
using CartService.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Infrastructure.Repositories
{
    public class CartRepository : EfCoreGenericRepository<Cart>, ICartRepository
    {
        public CartRepository(CartContext context) : base(context)
        {
            
        }

        public async Task<Cart?> GetByUserId(Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.CustomerId == userId);
        }
    }
}
