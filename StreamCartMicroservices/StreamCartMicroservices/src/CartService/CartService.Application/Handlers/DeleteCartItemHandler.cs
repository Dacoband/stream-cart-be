using CartService.Application.Command;
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
    public class DeleteCartItemHandler : IRequestHandler<DeleteCartItemCommand, ApiResponse<bool>>
    {
        private readonly ICartService _cartService;
        public DeleteCartItemHandler(ICartService cartService)
        {
            _cartService = cartService;
        }
        public async Task<ApiResponse<bool>> Handle(DeleteCartItemCommand request, CancellationToken cancellationToken)
        {
            return await _cartService.DeleteCart(request.CartItemId);
        }
    }
}
