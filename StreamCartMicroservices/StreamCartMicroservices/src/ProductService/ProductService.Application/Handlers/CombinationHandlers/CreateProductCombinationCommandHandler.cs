using MediatR;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Application.DTOs.Combinations;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CombinationHandlers
{
    public class CreateProductCombinationCommandHandler : IRequestHandler<CreateProductCombinationCommand, ProductCombinationDto>
    {
        private readonly IProductCombinationRepository _combinationRepository;
        private readonly IProductVariantRepository _variantRepository;
        private readonly IAttributeValueRepository _valueRepository;
        private readonly IProductAttributeRepository _attributeRepository;

        public CreateProductCombinationCommandHandler(
            IProductCombinationRepository combinationRepository,
            IProductVariantRepository variantRepository,
            IAttributeValueRepository valueRepository,
            IProductAttributeRepository attributeRepository)
        {
            _combinationRepository = combinationRepository ?? throw new ArgumentNullException(nameof(combinationRepository));
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<ProductCombinationDto> Handle(CreateProductCombinationCommand request, CancellationToken cancellationToken)
        {
            // Check if variant exists
            var variant = await _variantRepository.GetByIdAsync(request.VariantId.ToString());
            if (variant == null)
            {
                throw new ApplicationException($"Product variant with ID {request.VariantId} not found");
            }

            // Check if attribute value exists
            var attributeValue = await _valueRepository.GetByIdAsync(request.AttributeValueId.ToString());
            if (attributeValue == null)
            {
                throw new ApplicationException($"Attribute value with ID {request.AttributeValueId} not found");
            }

            // Check if the combination already exists
            if (await _combinationRepository.ExistsByVariantIdAndAttributeValueIdAsync(request.VariantId, request.AttributeValueId))
            {
                throw new ApplicationException($"A combination with variant ID {request.VariantId} and attribute value ID {request.AttributeValueId} already exists");
            }

            // Get the attribute name
            var attribute = await _attributeRepository.GetByIdAsync(attributeValue.AttributeId.ToString());
            if (attribute == null)
            {
                throw new ApplicationException($"Attribute with ID {attributeValue.AttributeId} not found");
            }

            // Create the combination
            var combination = new ProductCombination(request.VariantId, request.AttributeValueId, request.CreatedBy);

            // Save to database
            await _combinationRepository.InsertAsync(combination);

            // Return DTO
            return new ProductCombinationDto
            {
                VariantId = combination.VariantId,
                AttributeValueId = combination.AttributeValueId,
                AttributeName = attribute.Name,
                ValueName = attributeValue.ValueName,
                CreatedAt = combination.CreatedAt,
                CreatedBy = combination.CreatedBy,
                LastModifiedAt = combination.LastModifiedAt,
                LastModifiedBy = combination.LastModifiedBy
            };
        }
    }
}