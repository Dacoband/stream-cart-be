using CartService.Application.Command;
using CartService.Application.DTOs;
using CartService.Application.Interfaces;
using CartService.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Handlers
{
    public class AddToCartHandler : IRequestHandler<AddToCartCommand, bool>
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _itemRepository;
        private readonly IProductService _productService;
        public AddToCartHandler(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductService productService)
        {
            _cartRepository = cartRepository;
            _itemRepository = cartItemRepository;
            _productService = productService;
        }

        public Task<bool> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
