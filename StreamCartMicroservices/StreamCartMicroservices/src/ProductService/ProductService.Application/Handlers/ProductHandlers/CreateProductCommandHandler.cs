using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
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
        private readonly IShopServiceClient _shopServiceClient;

        public CreateProductCommandHandler(IProductRepository productRepository, IShopServiceClient shopServiceClient)
        {
            _productRepository = productRepository;
            _shopServiceClient = shopServiceClient;
        }

        public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra SKU trùng lặp nếu có
            if (!string.IsNullOrWhiteSpace(request.SKU) && !await _productRepository.IsSkuUniqueAsync(request.SKU))
            {
                throw new ApplicationException($"SKU '{request.SKU}' already exists");
            }
            if (request.ShopId.HasValue)
            {
                bool shopExists = await _shopServiceClient.DoesShopExistAsync(request.ShopId.Value);
                if (!shopExists)
                {
                    throw new ApplicationException($"Shop with ID {request.ShopId.Value} not found");
                }
            }

            // Tạo đối tượng sản phẩm
            var product = new Product(
                request.ProductName ?? string.Empty,
                request.Description ?? string.Empty,
                request.SKU ?? string.Empty,
                request.CategoryId,
                request.BasePrice,
                request.StockQuantity,
                request.ShopId);

            // Thiết lập thuộc tính nâng cao
            if (request.DiscountPrice.HasValue)
            {
                product.UpdatePricing(request.BasePrice, request.DiscountPrice);
            }

            
                product.UpdatePhysicalAttributes(request.Weight, request.Length, request.Width, request.Height); // Fix: Ensure 'dimensions' is not null
            

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
                Length = product.Length,
                Width = product.Width,
                Height = product.Height,
                HasVariant = product.HasVariant,
                QuantitySold = product.QuantitySold,
                ShopId = product.ShopId,
                CreatedAt = product.CreatedAt,
                CreatedBy = product.CreatedBy,
                LastModifiedAt = product.LastModifiedAt,
                LastModifiedBy = product.LastModifiedBy
            };
        }
    }
}