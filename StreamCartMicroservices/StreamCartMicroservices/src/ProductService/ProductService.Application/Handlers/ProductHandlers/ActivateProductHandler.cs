using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class ActivateProductHandler : IRequestHandler<AcctivateProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;
        public ActivateProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<bool> Handle(AcctivateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id.ToString());


            if (product == null)
            {
                return false;
            }
            product.Delete(request.ModifiedBy);

            if (!string.IsNullOrEmpty(request.ModifiedBy))
            {
                product.SetUpdatedBy(request.ModifiedBy);
            }
            await _productRepository.ReplaceAsync(product.Id.ToString(), product);

            return true;
        }
    }
}
