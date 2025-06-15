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
    public class CartItemRepository : EfCoreGenericRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(CartContext context) : base(context)
        {
            
        }
        public async Task<CartItem?> GetItemByCartId(Guid cartId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.CartId == cartId);
        }

    }
}
