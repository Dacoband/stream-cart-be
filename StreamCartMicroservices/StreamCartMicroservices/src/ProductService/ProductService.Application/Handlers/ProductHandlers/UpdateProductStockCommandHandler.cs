using MassTransit;
using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Application.DTOs;
using ProductService.Infrastructure.Interfaces;
using ProductService.Infrastructure.Repositories;
using Shared.Messaging.Event.ProductEvent;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class UpdateProductStockCommandHandler : IRequestHandler<UpdateProductStockCommand, ProductDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IProductImageRepository _productImageRepository;
        public UpdateProductStockCommandHandler(IProductRepository productRepository, IPublishEndpoint publishEndpoint, IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _publishEndpoint = publishEndpoint;
            _productImageRepository = productImageRepository;
        }

        public async Task<ProductDto> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id.ToString());

            if (product == null)
            {
                throw new ApplicationException($"Product with ID {request.Id} not found");
            }

            // Cập nhật số lượng tồn kho
            product.UpdateStock(request.StockQuantity);

            // Cập nhật người sửa đổi
            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                product.SetUpdatedBy(request.UpdatedBy);
            }

            await _productRepository.ReplaceAsync(product.Id.ToString(), product);
            try
            {
                var productEvent = new ProductUpdatedEvent()
                {
                    ProductId = product.Id,
                    ProductName = product.ProductName,
                    Price = (decimal)(product.DiscountPrice > 0 ? product.DiscountPrice : product.BasePrice),
                    Stock = product.StockQuantity,
                    ProductStatus = product.IsActive,

                };
                await _publishEndpoint.Publish(productEvent);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            decimal finalPrice = product.BasePrice;
            if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
            {
                // Apply discount as a percentage of original price
                finalPrice = product.BasePrice * (1 - (product.DiscountPrice.Value / 100));
            }

            // Get primary image if exists
            var primaryImage = await _productImageRepository.GetPrimaryImageAsync(product.Id);
            string? primaryImageUrl = primaryImage?.ImageUrl;

            return new ProductDto
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Description = product.Description,
                SKU = product.SKU,
                CategoryId = product.CategoryId,
                BasePrice = product.BasePrice,
                DiscountPrice = product.DiscountPrice,
                FinalPrice = finalPrice, // New field for final price after discount
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                HasVariant = product.HasVariant,
                QuantitySold = product.QuantitySold,
                ShopId = product.ShopId,
                // LivestreamId = product.LivestreamId,
                PrimaryImageUrl = primaryImageUrl, // New primary image URL field
                HasPrimaryImage = primaryImage != null,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
                LastModifiedAt = product.LastModifiedAt,
                LastModifiedBy = product.LastModifiedBy ?? string.Empty
            };
        }
    }
}