using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using CartService.Application.Query;
using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Handlers
{
    public class GetMyCartHandler : IRequestHandler<GetMyCartQuery, ApiResponse<CartResponeDTO>>
    {
        private readonly ICartService _cartService;
        public GetMyCartHandler(ICartService cartService)
        {
            _cartService = cartService;
        }
        public async Task<ApiResponse<CartResponeDTO>> Handle(GetMyCartQuery request, CancellationToken cancellationToken)
        {
            return await _cartService.GetMyCart(request.userId);
        }
    }
}
