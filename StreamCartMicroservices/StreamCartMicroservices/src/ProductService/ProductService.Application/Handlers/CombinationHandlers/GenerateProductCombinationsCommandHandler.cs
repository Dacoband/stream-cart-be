using MediatR;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Application.DTOs.Combinations;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CombinationHandlers
{
    public class GenerateProductCombinationsCommandHandler : IRequestHandler<GenerateProductCombinationsCommand, bool>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IAttributeValueRepository _valueRepository;
        private readonly IProductCombinationRepository _combinationRepository;

        public GenerateProductCombinationsCommandHandler(
            IProductRepository productRepository,
            IProductVariantRepository variantRepository,
            IAttributeValueRepository valueRepository,
            IProductCombinationRepository combinationRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
            _combinationRepository = combinationRepository ?? throw new ArgumentNullException(nameof(combinationRepository));
        }

        public async Task<bool> Handle(GenerateProductCombinationsCommand request, CancellationToken cancellationToken)
        {
            // Check if product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId.ToString());
            if (product == null)
            {
                throw new ApplicationException($"Product with ID {request.ProductId} not found");
            }

            // Make sure the product has variants enabled
            if (!product.HasVariant)
            {
                product.SetHasVariant(true);
                await _productRepository.ReplaceAsync(product.Id.ToString(), product);
            }

            // Validate attribute value groups
            if (request.AttributeValueGroups == null || !request.AttributeValueGroups.Any())
            {
                throw new ArgumentException("At least one attribute value group is required");
            }

            // Generate all possible combinations
            var combinations = GenerateCombinations(request.AttributeValueGroups);

            // For each combination, create a variant with associations to attribute values
            foreach (var combination in combinations)
            {
                // Generate a descriptive SKU for this variant
                string variantSku = await GenerateVariantSkuAsync(product.SKU, combination);

                // Create the variant
                var variant = new ProductVariant(
                    request.ProductId,
                    variantSku,
                    request.DefaultPrice,
                    request.DefaultStock,
                    request.CreatedBy);
                variant.Width = request.Width;
                variant.Height = request.Height;
                variant.Length = request.Length;
                variant.Weight = request.Weight;
                

                // Save the variant
                await _variantRepository.InsertAsync(variant);

                // Create combinations for this variant
                foreach (var attributeValueId in combination)
                {
                    var productCombination = new ProductCombination(variant.Id, attributeValueId, request.CreatedBy);
                    await _combinationRepository.InsertAsync(productCombination);
                }
            }

            return true;
        }

        private List<List<Guid>> GenerateCombinations(List<AttributeValueGroup> attributeValueGroups)
        {
            if (attributeValueGroups.Count == 0)
            {
                return new List<List<Guid>>();
            }

            if (attributeValueGroups.Count == 1)
            {
                return attributeValueGroups[0].AttributeValueIds.Select(id => new List<Guid> { id }).ToList();
            }

            var firstGroup = attributeValueGroups[0];
            var remainingGroups = attributeValueGroups.Skip(1).ToList();
            var remainingCombinations = GenerateCombinations(remainingGroups);

            var result = new List<List<Guid>>();
            foreach (var valueId in firstGroup.AttributeValueIds)
            {
                foreach (var combination in remainingCombinations)
                {
                    var newCombination = new List<Guid> { valueId };
                    newCombination.AddRange(combination);
                    result.Add(newCombination);
                }
            }

            return result;
        }

        private async Task<string> GenerateVariantSkuAsync(string baseSku, List<Guid> attributeValueIds)
        {
            var skuParts = new List<string> { baseSku };

            foreach (var attributeValueId in attributeValueIds)
            {
                var attributeValue = await _valueRepository.GetByIdAsync(attributeValueId.ToString());
                if (attributeValue != null)
                {
                    // Take first 3 chars of attribute value name, uppercased
                    string valuePart = attributeValue.ValueName.Length > 3
                        ? attributeValue.ValueName.Substring(0, 3).ToUpper()
                        : attributeValue.ValueName.ToUpper();

                    skuParts.Add(valuePart);
                }
            }

            return string.Join("-", skuParts);
        }
    }
}