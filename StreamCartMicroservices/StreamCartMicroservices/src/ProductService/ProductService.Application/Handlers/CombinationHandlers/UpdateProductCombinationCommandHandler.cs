using MassTransit;
using MediatR;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Application.DTOs.Combinations;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using Shared.Messaging.Event.ProductEvent;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CombinationHandlers
{
    public class UpdateProductCombinationCommandHandler : IRequestHandler<UpdateProductCombinationCommand, ProductCombinationDto>
    {
        private readonly IProductCombinationRepository _combinationRepository;
        private readonly IAttributeValueRepository _valueRepository;
        private readonly IProductAttributeRepository _attributeRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IProductVariantRepository _variantRepository;

        public UpdateProductCombinationCommandHandler(
            IProductCombinationRepository combinationRepository,
            IAttributeValueRepository valueRepository,
            IProductAttributeRepository attributeRepository, IPublishEndpoint publishEndpoint, IProductVariantRepository variantRepository)
        {
            _combinationRepository = combinationRepository ?? throw new ArgumentNullException(nameof(combinationRepository));
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
            _publishEndpoint = publishEndpoint;
            _variantRepository = variantRepository;
        }

        public async Task<ProductCombinationDto> Handle(UpdateProductCombinationCommand request, CancellationToken cancellationToken)
        {
            // Ensure UpdatedBy is not null or empty
            var updatedBy = string.IsNullOrWhiteSpace(request.UpdatedBy) ? "system" : request.UpdatedBy;

            // First, check if current combination exists
            var combinations = await _combinationRepository.GetByVariantIdAsync(request.CurrentVariantId);
            var existingCombination = combinations.FirstOrDefault(c =>
                c.VariantId == request.CurrentVariantId &&
                c.AttributeValueId == request.CurrentAttributeValueId &&
                !c.IsDeleted);

            if (existingCombination == null)
            {
                throw new ApplicationException($"Combination with variant ID {request.CurrentVariantId} and attribute value ID {request.CurrentAttributeValueId} not found");
            }

            // Check if the new attribute value exists
            var newAttributeValue = await _valueRepository.GetByIdAsync(request.NewAttributeValueId.ToString());
            if (newAttributeValue == null)
            {
                throw new ApplicationException($"Attribute value with ID {request.NewAttributeValueId} not found");
            }

            // Get the attribute for the new value
            var attribute = await _attributeRepository.GetByIdAsync(newAttributeValue.AttributeId.ToString());
            if (attribute == null)
            {
                throw new ApplicationException($"Attribute with ID {newAttributeValue.AttributeId} not found");
            }

            // Check if the combination with the new attribute value already exists
            if (await _combinationRepository.ExistsByVariantIdAndAttributeValueIdAsync(request.CurrentVariantId, request.NewAttributeValueId))
            {
                throw new ApplicationException($"A combination with variant ID {request.CurrentVariantId} and new attribute value ID {request.NewAttributeValueId} already exists");
            }

            // Delete the old combination
            await _combinationRepository.DeleteByVariantIdAndAttributeValueIdAsync(
                request.CurrentVariantId, request.CurrentAttributeValueId);

            // Create the new combination
            var newCombination = new ProductCombination(
                request.CurrentVariantId,
                request.NewAttributeValueId,
                updatedBy); // Use the validated updatedBy value

            await _combinationRepository.InsertAsync(newCombination);
            try
            {
                var attributeDict = new Dictionary<string, string>
                {
                    { attribute.Id.ToString(), newAttributeValue.Id.ToString() }
                };
                var variantDetail = await _variantRepository.GetByIdAsync(newCombination.VariantId.ToString());

                var productEvent = new ProductUpdatedEvent()
                {
                    ProductId = variantDetail.ProductId,
                    VariantId = newCombination.VariantId,
                    Attributes = attributeDict
                };
                await _publishEndpoint.Publish(productEvent);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            // Return the updated combination as DTO
            return new ProductCombinationDto
            {
                VariantId = newCombination.VariantId,
                AttributeValueId = newCombination.AttributeValueId,
                AttributeName = attribute.Name,
                ValueName = newAttributeValue.ValueName,
                CreatedAt = newCombination.CreatedAt,
                CreatedBy = newCombination.CreatedBy,
                LastModifiedAt = newCombination.LastModifiedAt,
                LastModifiedBy = newCombination.LastModifiedBy ?? string.Empty
            };
        }
    }
}
