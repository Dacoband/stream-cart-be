using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
    {
        private readonly IProductRepository _productRepository;

        public CreateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra SKU trùng lặp nếu có
            if (!string.IsNullOrWhiteSpace(request.SKU) && !await _productRepository.IsSkuUniqueAsync(request.SKU))
            {
                throw new ApplicationException($"SKU '{request.SKU}' already exists");
            }

            // Tạo đối tượng sản phẩm
            var product = new Product(
                request.ProductName,
                request.Description,
                request.SKU,
                request.CategoryId,
                request.BasePrice,
                request.StockQuantity,
                request.ShopId);

            // Thiết lập thuộc tính nâng cao
            if (request.DiscountPrice.HasValue)
            {
                product.UpdatePricing(request.BasePrice, request.DiscountPrice);
            }

            if (request.Weight.HasValue || !string.IsNullOrWhiteSpace(request.Dimensions))
            {
                product.UpdatePhysicalAttributes(request.Weight, request.Dimensions);
            }

            if (request.HasVariant)
            {
                product.SetHasVariant(true);
            }

            // Thiết lập người tạo
            if (!string.IsNullOrWhiteSpace(request.CreatedBy))
            {
                // Use the BaseEntity methods instead of direct property assignment
                product.SetCreator(request.CreatedBy);
                product.SetModifier(request.CreatedBy);
            }

            // Lưu vào database
            await _productRepository.InsertAsync(product);

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
                LivestreamId = product.LivestreamId,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
                LastModifiedAt = product.LastModifiedAt,
                LastModifiedBy = product.LastModifiedBy
            };
        }
    }
}