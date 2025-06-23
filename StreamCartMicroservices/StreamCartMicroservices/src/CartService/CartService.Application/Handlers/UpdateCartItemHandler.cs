using CartService.Application.Command;
using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Handlers
{
    public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, ApiResponse<UpdateCartItemDTO>>
    {
        private readonly ICartService _cartService;
        public UpdateCartItemHandler(ICartService cartService)
        {
            _cartService = cartService;
        }
        public async Task<ApiResponse<UpdateCartItemDTO>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            UpdateCartItemDTO cartItemDTO = new UpdateCartItemDTO()
            {
                CartItem = request.CartItemId,
                Quantity = request.Quantity,
                VariantId = request.VariantId,
            };
            return await _cartService.UpdateCartItem(cartItemDTO, request.UserId);
        }
    }
}
