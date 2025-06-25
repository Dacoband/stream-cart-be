using CartService.Application.Command;
using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using CartService.Infrastructure.Interfaces;
using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Handlers
{
    public class AddToCartHandler : IRequestHandler<AddToCartCommand, ApiResponse<CreateCartDTO>>
    {
        private  readonly ICartService _cartService;
        public AddToCartHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<ApiResponse<CreateCartDTO>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            CreateCartDTO createCartDTO = new CreateCartDTO()
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Quantity = request.Quantity,
            };
            return await _cartService.AddToCart(createCartDTO, request.UserId);
        }
    }
}
