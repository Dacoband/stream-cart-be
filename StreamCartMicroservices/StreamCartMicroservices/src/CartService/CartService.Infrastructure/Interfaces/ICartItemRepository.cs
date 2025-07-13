using CartService.Domain.Entities;
using Shared.Common.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Infrastructure.Interfaces
{
    public interface ICartItemRepository : IGenericRepository<CartItem>
    {
        Task<CartItem?> GetItemByCartId(Guid cartId);
        Task<List<CartItem>> GetCartByProduct(Guid productId, Guid? variantId);
        Task<List<CartItem>> GetCartItemByShop(Guid shopId);
        Task DeleteCartItem(Guid cartId);

    }
}
