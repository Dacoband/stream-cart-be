using MediatR;
using ProductService.Application.Commands.AttributeValueCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeValueHandlers
{
    public class CreateAttributeValueCommandHandler : IRequestHandler<CreateAttributeValueCommand, AttributeValueDto>
    {
        private readonly IAttributeValueRepository _valueRepository;
        private readonly IProductAttributeRepository _attributeRepository;

        public CreateAttributeValueCommandHandler(
            IAttributeValueRepository valueRepository,
            IProductAttributeRepository attributeRepository)
        {
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<AttributeValueDto> Handle(CreateAttributeValueCommand request, CancellationToken cancellationToken)
        {
            // Validate request.ValueName to ensure it is not null or empty
            if (string.IsNullOrWhiteSpace(request.ValueName))
            {
                throw new ArgumentException("ValueName cannot be null or empty", nameof(request.ValueName));
            }

            // Validate request.CreatedBy to ensure it is not null or empty
            if (string.IsNullOrWhiteSpace(request.CreatedBy))
            {
                throw new ArgumentException("CreatedBy cannot be null or empty", nameof(request.CreatedBy));
            }

            // Check if attribute exists
            var attribute = await _attributeRepository.GetByIdAsync(request.AttributeId.ToString());
            if (attribute == null)
            {
                throw new ApplicationException($"Attribute with ID {request.AttributeId} not found");
            }

            // Check if value name is unique for this attribute
            if (await _valueRepository.IsValueNameUniqueForAttributeAsync(request.AttributeId, request.ValueName) == false)
            {
                throw new ApplicationException($"Value '{request.ValueName}' already exists for this attribute");
            }

            // Create the attribute value
            var attributeValue = new AttributeValue(request.AttributeId, request.ValueName, request.CreatedBy!);

            // Save to database
            await _valueRepository.InsertAsync(attributeValue);

            // Return DTO
            return new AttributeValueDto
            {
                Id = attributeValue.Id,
                AttributeId = attributeValue.AttributeId,
                ValueName = attributeValue.ValueName,
                CreatedAt = attributeValue.CreatedAt,
                CreatedBy = attributeValue.CreatedBy,
                LastModifiedAt = attributeValue.LastModifiedAt,
                LastModifiedBy = attributeValue.LastModifiedBy
            };
        }
    }
}