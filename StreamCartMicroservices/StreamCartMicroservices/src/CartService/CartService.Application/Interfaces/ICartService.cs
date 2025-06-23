using CartService.Application.DTOs;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Interfaces
{
    public interface ICartService
    {
        public Task<ApiResponse<CreateCartDTO>> AddToCart(CreateCartDTO cart, string userId);
        public Task<ApiResponse<CartResponeDTO>> GetMyCart(string userId);
        public Task<ApiResponse<PreviewOrderResponseDTO>> PreviewOrder(PreviewOrderRequestDTO order);
        public Task<ApiResponse<bool>> DeleteCart(Guid cartItemId);
        public Task<ApiResponse<UpdateCartItemDTO>> UpdateCartItem(UpdateCartItemDTO cartItem, string userId);
    }
}
