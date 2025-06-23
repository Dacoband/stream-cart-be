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
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateProductCommandHandler(IProductRepository productRepository, IPublishEndpoint publishEndpoint)
        {
            _productRepository = productRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            // Validate required fields to prevent null reference issues
            if (string.IsNullOrWhiteSpace(request.ProductName))
            {
                throw new ArgumentException("ProductName cannot be null or empty", nameof(request.ProductName));
            }

            if (string.IsNullOrWhiteSpace(request.Description))
            {
                throw new ArgumentException("Description cannot be null or empty", nameof(request.Description));
            }

            if (string.IsNullOrWhiteSpace(request.SKU))
            {
                throw new ArgumentException("SKU cannot be null or empty", nameof(request.SKU));
            }

            // Validate dimensions to prevent null reference issues
            if (request.Dimensions == null)
            {
                throw new ArgumentException("Dimensions cannot be null", nameof(request.Dimensions));
            }

            // Tìm sản phẩm dựa trên ID
            var product = await _productRepository.GetByIdAsync(request.Id.ToString());

            if (product == null)
            {
                throw new ApplicationException($"Product with ID {request.Id} not found");
            }

            // Kiểm tra SKU trùng lặp nếu có cập nhật
            if (!string.IsNullOrWhiteSpace(request.SKU) && request.SKU != product.SKU &&
                !await _productRepository.IsSkuUniqueAsync(request.SKU, request.Id))
            {
                throw new ApplicationException($"SKU '{request.SKU}' already exists");
            }

            // Cập nhật thông tin cơ bản
            product.UpdateBasicInfo(
                request.ProductName,
                request.Description,
                request.SKU,
                request.CategoryId);

            // Cập nhật giá nếu có
            if (request.BasePrice.HasValue)
            {
                product.UpdatePricing(
                    request.BasePrice.Value,
                    request.DiscountPrice);
            }

            // Cập nhật thuộc tính vật lý
            product.UpdatePhysicalAttributes(
                request.Weight,
                request.Dimensions!); // Use null-forgiving operator since null is already validated

            // Cập nhật tùy chọn biến thể
            if (request.HasVariant.HasValue)
            {
                product.SetHasVariant(request.HasVariant.Value);
            }

            // Cập nhật người sửa đổi
            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                product.SetUpdatedBy(request.UpdatedBy);
            }

            // Lưu thay đổi
            await _productRepository.ReplaceAsync(product.Id.ToString(), product);
            var productEvent = new ProductUpdatedEvent()
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Price = (decimal)(product.DiscountPrice > 0 ? product.DiscountPrice : product.BasePrice),
                Stock = product.StockQuantity,

            };
            try
            {
                await _publishEndpoint.Publish(productEvent);
            }
            catch (Exception ex) { 
            
                throw ex;
            }

            // Trả về DTO
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
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
                LastModifiedAt = product.LastModifiedAt,
                LastModifiedBy = product.LastModifiedBy ?? string.Empty
            };
        }
    }
}