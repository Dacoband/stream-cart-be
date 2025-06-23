using MassTransit;
using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Application.DTOs;
using ProductService.Infrastructure.Interfaces;
using Shared.Messaging.Event.ProductEvent;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, ProductDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateProductStatusCommandHandler(IProductRepository productRepository, IPublishEndpoint publishEndpoint)
        {
            _productRepository = productRepository;
            _publishEndpoint = publishEndpoint;
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