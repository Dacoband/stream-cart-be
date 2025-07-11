using MediatR;
using ProductService.Application.DTOs.Products;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.DetailQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.DetailHandlers
{
    public class GetProductDetailQueryHandler : IRequestHandler<GetProductDetailQuery, ProductDetailDto?>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductImageRepository _imageRepository;
        private readonly IProductAttributeRepository _attributeRepository;
        private readonly IAttributeValueRepository _attributeValueRepository;
        private readonly IProductCombinationRepository _combinationRepository;
        private readonly IShopServiceClient _shopServiceClient;

        public GetProductDetailQueryHandler(
            IProductRepository productRepository,
            IProductVariantRepository variantRepository,
            IProductImageRepository imageRepository,
            IProductAttributeRepository attributeRepository,
            IAttributeValueRepository attributeValueRepository,
            IProductCombinationRepository combinationRepository,
            IShopServiceClient shopServiceClient)
        {
            _productRepository = productRepository;
            _variantRepository = variantRepository;
            _imageRepository = imageRepository;
            _attributeRepository = attributeRepository;
            _attributeValueRepository = attributeValueRepository;
            _combinationRepository = combinationRepository;
            _shopServiceClient = shopServiceClient;
        }

        public async Task<ProductDetailDto?> Handle(GetProductDetailQuery request, CancellationToken cancellationToken)
        {
            // Get product data
            var product = await _productRepository.GetByIdAsync(request.ProductId.ToString());
            if (product == null || product.IsDeleted)
                return null;

            // Get product images
            var images = await _imageRepository.GetByProductIdAsync(product.Id);
            var primaryImages = images
                .Where(i => i.IsPrimary)
                .Select(i => i.ImageUrl)
                .ToList();

            // Get product variants
            var variants = await _variantRepository.GetByProductIdAsync(product.Id);

            // Build attributes and variants collection
            var attributes = new Dictionary<Guid, string>(); // AttributeId -> Name
            var attributeValues = new Dictionary<Guid, (Guid AttributeId, string ValueName)>(); // ValueId -> (AttributeId, Name)
            var variantAttributeValues = new Dictionary<Guid, List<(string AttributeName, string ValueName)>>(); // VariantId -> List of (AttributeName, ValueName)

            // Process each variant to collect attribute data
            foreach (var variant in variants)
            {
                var combinations = await _combinationRepository.GetByVariantIdAsync(variant.Id);
                var attributeValuePairs = new List<(string AttributeName, string ValueName)>();

                foreach (var combo in combinations)
                {
                    var value = await _attributeValueRepository.GetByIdAsync(combo.AttributeValueId.ToString());
                    if (value == null) continue;

                    var attribute = await _attributeRepository.GetByIdAsync(value.AttributeId.ToString());
                    if (attribute == null) continue;

                    // Add to our tracking dictionaries
                    if (!attributes.ContainsKey(attribute.Id))
                        attributes[attribute.Id] = attribute.Name;

                    if (!attributeValues.ContainsKey(value.Id))
                        attributeValues[value.Id] = (attribute.Id, value.ValueName);

                    attributeValuePairs.Add((attribute.Name, value.ValueName));
                }

                variantAttributeValues[variant.Id] = attributeValuePairs;
            }

            // Group attributes with their values
            var attributeDtos = new List<ProductDetailAttributeDto>();
            var attrValuesByAttr = attributeValues
                .GroupBy(av => av.Value.AttributeId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Value.ValueName).Distinct().ToList());

            var attributeValueImages = new Dictionary<string, string>();
            foreach (var image in images)
            {
                if (image.VariantId.HasValue)
                {
                    var variantCombinations = await _combinationRepository.GetByVariantIdAsync(image.VariantId.Value);
                    foreach (var combo in variantCombinations)
                    {
                        var value = await _attributeValueRepository.GetByIdAsync(combo.AttributeValueId.ToString());
                        if (value != null)
                        {
                            attributeValueImages[value.ValueName] = image.ImageUrl;
                        }
                    }
                }
            }
            foreach (var attr in attributes)
            {
                var valueNames = attrValuesByAttr.ContainsKey(attr.Key) ? attrValuesByAttr[attr.Key] : new List<string>();
                var valuePairs = new List<AttributeValueImagePair>();

                // Create explicit value-image pairs
                foreach (var valueName in valueNames)
                {
                    valuePairs.Add(new AttributeValueImagePair
                    {
                        Value = valueName,
                        ImageUrl = attributeValueImages.ContainsKey(valueName)
                            ? attributeValueImages[valueName]
                            : string.Empty
                    });
                }

                attributeDtos.Add(new ProductDetailAttributeDto
                {
                    AttributeName = attr.Value,
                    ValueImagePairs = valuePairs
                });
            }
            decimal finalPrice = product.BasePrice;
            if (product.DiscountPrice.HasValue && product.DiscountPrice.Value > 0)
            {
                // Apply discount as a percentage of original price
                finalPrice = product.BasePrice * (1 - (product.DiscountPrice.Value / 100));
            }
            // Build variant DTOs
            var variantDtos = new List<ProductDetailVariantDto>();
            foreach (var variant in variants)
            {
                // Get variant image
                var variantImage = images
                    .FirstOrDefault(i => i.VariantId == variant.Id && i.IsPrimary);

                var variantImageDto = variantImage != null
                    ? new ProductDetailVariantImageDto
                    {
                        ImageId = variantImage.Id,
                        Url = variantImage.ImageUrl,
                        AltText = variantImage.AltText ?? variant.SKU
                    }
                    : null;

                // Get attribute values for this variant
                var attributeValueDict = new Dictionary<string, string>();
                if (variantAttributeValues.ContainsKey(variant.Id))
                {
                    foreach (var pair in variantAttributeValues[variant.Id])
                    {
                        attributeValueDict[pair.AttributeName] = pair.ValueName;
                    }
                }

                variantDtos.Add(new ProductDetailVariantDto
                {
                    VariantId = variant.Id,
                    AttributeValues = attributeValueDict,
                    Stock = variant.Stock,
                    Price = variant.Price,
                    FlashSalePrice = variant.FlashSalePrice,
                    VariantImage = variantImageDto
                });
            }

            // Get shop info (placeholder - in production you'd call a shop service)
            var shopInfo = product.ShopId.HasValue ?
                await _shopServiceClient.GetShopByIdAsyncDetail(product.ShopId.Value) :
                new  ShopDetailDto
                {
                    Id = Guid.Empty,
                    ShopName = "Unknown Shop",
                    RegistrationDate = DateTime.UtcNow,
                    CompleteRate = 0,
                    TotalReview = 0,
                    RatingAverage = 0,
                    LogoURL = string.Empty,
                    TotalProduct = 0
                };

            // Build final response
            return new ProductDetailDto
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = GetCategoryNamePlaceholder(product.CategoryId),
                BasePrice = product.BasePrice,
                DiscountPrice = product.DiscountPrice,
                FinalPrice = finalPrice,
                StockQuantity = product.StockQuantity,
                QuantitySold = product.QuantitySold,
                Weight = product.Weight.HasValue ? $"{product.Weight}g" : null,
                Dimension = product.Dimensions,
                PrimaryImage = primaryImages,

                ShopId = product.ShopId ?? Guid.Empty,
                ShopName = shopInfo.ShopName,
                ShopStartTime = shopInfo.ApprovalDate ?? shopInfo.RegistrationDate,
                ShopCompleteRate = shopInfo.CompleteRate,
                ShopTotalReview = shopInfo.TotalReview,
                ShopRatingAverage = shopInfo.RatingAverage,
                ShopLogo = shopInfo.LogoURL,
                ShopTotalProduct = shopInfo.TotalProduct,

                Attributes = attributeDtos,
                Variants = variantDtos
            };
        }

        // Temporary method to return placeholder category info
        private string GetCategoryNamePlaceholder(Guid? categoryId)
        {
            return categoryId.HasValue ? "Thời trang" : "Uncategorized";
        }

        // Temporary method to return placeholder shop info
        private dynamic GetShopInfoPlaceholder(Guid? shopId)
        {
            if (!shopId.HasValue)
            {
                return new
                {
                    Name = "Unknown Shop",
                    StartTime = DateTime.UtcNow,
                    CompleteRate = 0m,
                    TotalReview = 0,
                    RatingAverage = 0m,
                    LogoUrl = string.Empty,
                    TotalProducts = 0
                };
            }

            return new
            {
                Name = "ShopABC",
                StartTime = new DateTime(2022, 11, 22),
                CompleteRate = 100m,
                TotalReview = 212,
                RatingAverage = 3.5m,
                LogoUrl = "abc.com",
                TotalProducts = 100
            };
        }
    }
}