using MassTransit;
using MediatR;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Variants;
using ProductService.Infrastructure.Interfaces;
using Shared.Messaging.Event.ProductEvent;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class UpdateVariantStockCommandHandler : IRequestHandler<UpdateVariantStockCommand, ProductVariantDto>
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IProductRepository _productRepository;
        public UpdateVariantStockCommandHandler(IProductVariantRepository variantRepository, IPublishEndpoint publishEndpoint, IProductRepository productRepository)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
            _publishEndpoint = publishEndpoint;
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<ProductVariantDto> Handle(UpdateVariantStockCommand request, CancellationToken cancellationToken)
        {
            var variant = await _variantRepository.GetByIdAsync(request.Id.ToString());
            if (variant == null)
            {
                throw new ApplicationException($"Product variant with ID {request.Id} not found");
            }

            variant.UpdateStock(request.Stock);

            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                variant.SetUpdatedBy(request.UpdatedBy);
            }

            await _variantRepository.ReplaceAsync(variant.Id.ToString(), variant);
            await UpdateProductStockFromVariants(variant.ProductId, request.UpdatedBy);
            try
            {
                var productEvent = new ProductUpdatedEvent()
                {
                    ProductId = variant.ProductId,
                    Price = (decimal)(variant.FlashSalePrice > 0 ? variant.FlashSalePrice : variant.Price),
                    Stock = variant.Stock,
                    ProductStatus = !variant.IsDeleted,
                    VariantId = variant.Id,
                };
                await _publishEndpoint.Publish(productEvent);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                SKU = variant.SKU,
                Price = variant.Price,
                FlashSalePrice = variant.FlashSalePrice,
                Stock = variant.Stock,
                CreatedAt = variant.CreatedAt,
                CreatedBy = variant.CreatedBy,
                LastModifiedAt = variant.LastModifiedAt,
                LastModifiedBy = variant.LastModifiedBy ?? string.Empty 
            };
        }
        private async Task UpdateProductStockFromVariants(Guid productId, string? updatedBy)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId.ToString());
                if (product == null) return;

                var allVariants = await _variantRepository.GetByProductIdAsync(productId);
                var totalStock = allVariants.Sum(v => v.Stock);

                product.UpdateStock(totalStock);

                if (!string.IsNullOrEmpty(updatedBy))
                {
                    product.SetUpdatedBy(updatedBy);
                }

                await _productRepository.ReplaceAsync(product.Id.ToString(), product);

                //var productEvent = new ProductUpdatedEvent()
                //{
                //    ProductId = product.Id,
                //    ProductName = product.ProductName,
                //    Price = (decimal)(product.DiscountPrice > 0 ? product.DiscountPrice : product.BasePrice),
                //    Stock = product.StockQuantity,
                //    ProductStatus = product.IsActive,
                //};
                //await _publishEndpoint.Publish(productEvent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating product stock from variants: {ex.Message}");
            }
        }
    }
}