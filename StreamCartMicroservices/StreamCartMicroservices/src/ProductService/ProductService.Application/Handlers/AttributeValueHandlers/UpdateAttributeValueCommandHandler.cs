using MediatR;
using ProductService.Application.Commands.AttributeValueCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeValueHandlers
{
    public class UpdateAttributeValueCommandHandler : IRequestHandler<UpdateAttributeValueCommand, AttributeValueDto>
    {
        private readonly IAttributeValueRepository _valueRepository;
        private readonly IProductAttributeRepository _attributeRepository;

        public UpdateAttributeValueCommandHandler(
            IAttributeValueRepository valueRepository,
            IProductAttributeRepository attributeRepository)
        {
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<AttributeValueDto> Handle(UpdateAttributeValueCommand request, CancellationToken cancellationToken)
        {
            var attributeValue = await _valueRepository.GetByIdAsync(request.Id.ToString());
            if (attributeValue == null)
            {
                throw new ApplicationException($"Attribute value with ID {request.Id} not found");
            }

            var attribute = await _attributeRepository.GetByIdAsync(attributeValue.AttributeId.ToString());
            if (attribute == null)
            {
                throw new ApplicationException($"Attribute with ID {attributeValue.AttributeId} not found");
            }

            // Check if value name is unique for this attribute if changed
            if (!string.IsNullOrEmpty(request.ValueName) &&
                attributeValue.ValueName != request.ValueName &&
                !await _valueRepository.IsValueNameUniqueForAttributeAsync(attributeValue.AttributeId, request.ValueName, request.Id))
            {
                throw new ApplicationException($"Value '{request.ValueName}' already exists for this attribute");
            }

            // Ensure ValueName is not null before calling UpdateValueName
            if (!string.IsNullOrEmpty(request.ValueName))
            {
                attributeValue.UpdateValueName(request.ValueName);
            }

            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                attributeValue.SetUpdatedBy(request.UpdatedBy);
            }

            await _valueRepository.ReplaceAsync(attributeValue.Id.ToString(), attributeValue);

            return new AttributeValueDto
            {
                Id = attributeValue.Id,
                AttributeId = attributeValue.AttributeId,
                ValueName = attributeValue.ValueName,
                CreatedAt = attributeValue.CreatedAt,
                CreatedBy = attributeValue.CreatedBy,
                LastModifiedAt = attributeValue.LastModifiedAt,
                LastModifiedBy = attributeValue.LastModifiedBy ?? string.Empty
            };
        }
    }
}