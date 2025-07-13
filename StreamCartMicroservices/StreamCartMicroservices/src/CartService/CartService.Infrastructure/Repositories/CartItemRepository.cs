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

        public async Task<List<CartItem>> GetCartByProduct(Guid productId, Guid? variantId)
        {
            var product =await _dbSet.Where(x => x.ProductId == productId && x.IsDeleted == false).ToListAsync();
            if (variantId.HasValue)
            {
                product = product.Where(x => x.VariantId == variantId).ToList();
            }
            return product;
        }

        public async Task<List<CartItem>> GetCartItemByShop(Guid shopId)
        {
            return await _dbSet.Where(x=> x.ShopId == shopId && x.IsDeleted == false).ToListAsync();
        }

        public async Task<CartItem?> GetItemByCartId(Guid cartId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.CartId == cartId);
        }

        public async Task  DeleteCartItem(Guid cartItemId)
        {
            var cartItem = await _dbSet.FirstOrDefaultAsync(x => x.Id == cartItemId);
             _dbSet.Remove(cartItem);
            await _dbContext.SaveChangesAsync();
            return;
        }

    }
}
