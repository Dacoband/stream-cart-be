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
    public class PreviewOrderHandler : IRequestHandler<PreviewOrderQuery, ApiResponse<PreviewOrderResponseDTO>>
    {
        private readonly ICartService _cartService;
        public PreviewOrderHandler(ICartService cartService)
        {
            _cartService = cartService;
        }
        public async Task<ApiResponse<PreviewOrderResponseDTO>> Handle(PreviewOrderQuery request, CancellationToken cancellationToken)
        {
            PreviewOrderRequestDTO requestDTO = new PreviewOrderRequestDTO()
            {
                CartItemId = request.CartItemId,
            };
            return await _cartService.PreviewOrder(requestDTO);
        }
    }
}
