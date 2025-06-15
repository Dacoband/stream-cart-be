using MediatR;
using ProductService.Application.Commands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id.ToString());

            if (product == null)
            {
                return false;
            }

            // Thực hiện xóa mềm
            product.Delete();

            if (!string.IsNullOrEmpty(request.DeletedBy))
            {
                product.SetUpdatedBy(request.DeletedBy);
            }

            await _productRepository.ReplaceAsync(product.Id.ToString(), product);

            return true;
        }
    }
}