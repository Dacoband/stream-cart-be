using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Application.DTOs;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, ProductDto>
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductStatusCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id.ToString());

            if (product == null)
            {
                throw new ApplicationException($"Product with ID {request.Id} not found");
            }

            // Cập nhật trạng thái
            if (request.IsActive)
            {
                product.Activate();
            }
            else
            {
                product.Deactivate();
            }

            // Cập nhật thông tin người sửa đổi
            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                product.SetUpdatedBy(request.UpdatedBy);
            }

            await _productRepository.ReplaceAsync(product.Id.ToString(), product);

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Description = product.Description,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                BasePrice = product.BasePrice,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                HasVariant = product.HasVariant,
                QuantitySold = product.QuantitySold,
                ShopId = product.ShopId,
                //LivestreamId = product.LivestreamId,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
                LastModifiedAt = product.LastModifiedAt,
                LastModifiedBy = product.LastModifiedBy ?? string.Empty
            };
        }
    }
}