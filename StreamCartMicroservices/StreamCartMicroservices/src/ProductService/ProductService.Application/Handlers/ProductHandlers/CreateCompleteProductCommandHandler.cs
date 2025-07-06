using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class CreateCompleteProductCommandHandler : IRequestHandler<CreateCompleteProductCommand, ProductDto>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductImageRepository _imageRepository;
        private readonly IProductAttributeRepository _attributeRepository;
        private readonly IAttributeValueRepository _attributeValueRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductCombinationRepository _combinationRepository;
        private readonly IShopServiceClient _shopServiceClient;

        public CreateCompleteProductCommandHandler(
            IProductRepository productRepository,
            IProductImageRepository imageRepository,
            IProductAttributeRepository attributeRepository,
            IAttributeValueRepository attributeValueRepository,
            IProductVariantRepository variantRepository,
            IProductCombinationRepository combinationRepository,
            IShopServiceClient shopServiceClient)
        {
            _productRepository = productRepository;
            _imageRepository = imageRepository;
            _attributeRepository = attributeRepository;
            _attributeValueRepository = attributeValueRepository;
            _variantRepository = variantRepository;
            _combinationRepository = combinationRepository;
            _shopServiceClient = shopServiceClient;
        }

        public async Task<ProductDto> Handle(CreateCompleteProductCommand request, CancellationToken cancellationToken)
        {
            var dto = request.CompleteProduct;
            var createdBy = request.CreatedBy;

            // 1. Validate base product data
            if (string.IsNullOrWhiteSpace(dto.ProductName))
                throw new ArgumentException("Tên sản phẩm là bắt buộc", nameof(dto.ProductName));

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Mô tả sản phẩm là bắt buộc", nameof(dto.Description));

            if (string.IsNullOrWhiteSpace(dto.SKU))
                throw new ArgumentException("SKU là bắt buộc", nameof(dto.SKU));

            // Check SKU uniqueness
            if (!await _productRepository.IsSkuUniqueAsync(dto.SKU))
                throw new ApplicationException($"SKU '{dto.SKU}' đã tồn tại");

            // Validate shop if provided
            if (dto.ShopId.HasValue)
            {
                bool shopExists = await _shopServiceClient.DoesShopExistAsync(dto.ShopId.Value);
                if (!shopExists)
                    throw new ApplicationException($"Shop với ID {dto.ShopId.Value} không tồn tại");
            }

            // 2. Create base product
            var product = new Product(
                dto.ProductName,
                dto.Description,
                dto.SKU,
                dto.CategoryId,
                dto.BasePrice,
                dto.StockQuantity,
                dto.ShopId);

            // Set HasVariant flag
            product.SetHasVariant(dto.HasVariant);

            // Set physical attributes
            if (dto.Weight.HasValue || !string.IsNullOrWhiteSpace(dto.Dimensions))
                product.UpdatePhysicalAttributes(dto.Weight, dto.Dimensions ?? string.Empty);

            // Set creator
            if (!string.IsNullOrWhiteSpace(createdBy))
            {
                product.SetCreator(createdBy);
                product.SetModifier(createdBy);
            }

            // Save product to get ID
            await _productRepository.InsertAsync(product);

            // 3. Create product images
            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var imageDto in dto.Images)
                {
                    var image = new ProductImage(
                        product.Id,
                        imageDto.ImageUrl ?? string.Empty ,
                        null, 
                        imageDto.IsPrimary,
                        imageDto.DisplayOrder,
                        imageDto.AltText,
                        createdBy);

                    await _imageRepository.InsertAsync(image);
                }
            }

            // 4. Create product attributes and values
            var attributeMap = new Dictionary<string, Guid>(); // Map attribute names to IDs
            var valueMap = new Dictionary<(string, string), Guid>(); // Map (attribute, value) pairs to IDs

            if (dto.Attributes != null && dto.Attributes.Any())
            {
                foreach (var attributeDto in dto.Attributes)
                {
                    // Check if attribute already exists
                    var existingAttribute = await _attributeRepository.FindOneAsync(a => a.Name == attributeDto.Name);

                    Guid attributeId;
                    if (existingAttribute == null)
                    {
                        // Create new attribute
                        var attribute = new ProductAttribute(attributeDto.Name ?? string.Empty, createdBy);
                        await _attributeRepository.InsertAsync(attribute);
                        attributeId = attribute.Id;
                    }
                    else
                    {
                        attributeId = existingAttribute.Id;
                    }

                    attributeMap[attributeDto.Name ?? string.Empty] = attributeId;

                    // Create attribute values
                    if (attributeDto.Values != null && attributeDto.Values.Any())
                    {
                        foreach (var valueName in attributeDto.Values)
                        {
                            // Check if value already exists
                            var existingValue = await _attributeValueRepository.FindOneAsync(
                                v => v.AttributeId == attributeId && v.ValueName == valueName);

                            Guid valueId;
                            if (existingValue == null)
                            {
                                // Create new value
                                var value = new AttributeValue(attributeId, valueName, createdBy);
                                await _attributeValueRepository.InsertAsync(value);
                                valueId = value.Id;
                            }
                            else
                            {
                                valueId = existingValue.Id;
                            }

                            valueMap[(attributeDto.Name ?? string.Empty, valueName)] = valueId;
                        }
                    }
                }
            }

            // 5. Create variants and combinations
            if (dto.HasVariant && dto.Variants != null && dto.Variants.Any())
            {
                foreach (var variantDto in dto.Variants)
                {
                    // Create variant
                    var variant = new ProductVariant(
                        product.Id,
                        variantDto.SKU ?? string.Empty,
                        variantDto.Price,
                        variantDto.Stock,
                        createdBy);

                    await _variantRepository.InsertAsync(variant);

                    // Create combinations
                    if (variantDto.Attributes != null && variantDto.Attributes.Any())
                    {
                        foreach (var variantAttr in variantDto.Attributes)
                        {
                            if (!attributeMap.TryGetValue(variantAttr.AttributeName ?? string.Empty, out var attributeId))
                                continue; // Skip if attribute doesn't exist

                            if (!valueMap.TryGetValue((variantAttr.AttributeName, variantAttr.AttributeValue), out var valueId))
                                continue; // Skip if value doesn't exist

                            // Create combination
                            var combination = new ProductCombination(variant.Id, valueId, createdBy);
                            await _combinationRepository.InsertAsync(combination);
                        }
                    }
                }
            }

            // Return product DTO
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
                LastModifiedBy = product.LastModifiedBy
            };
        }
    }
}