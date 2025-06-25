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

        public async Task<Cart?> GetMyCart(string customerId)
        {
            return await _dbSet.Where(x=> x.CreatedBy == customerId).Include(x=>x.Items).FirstOrDefaultAsync();
        }
    }
}
