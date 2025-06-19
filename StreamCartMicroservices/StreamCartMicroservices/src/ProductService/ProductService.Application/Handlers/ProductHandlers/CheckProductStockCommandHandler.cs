using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class CheckProductStockCommandHandler : IRequestHandler<CheckProductStockCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public CheckProductStockCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(CheckProductStockCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId.ToString());

            if (product == null)
            {
                throw new ApplicationException($"Product with ID {request.ProductId} not found");
            }

            return product.HasSufficientStock(request.RequestedQuantity);
        }
    }
}