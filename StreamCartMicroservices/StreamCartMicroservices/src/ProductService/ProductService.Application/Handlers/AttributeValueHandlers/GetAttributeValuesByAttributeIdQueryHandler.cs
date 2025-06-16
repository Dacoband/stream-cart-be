using MediatR;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Queries.AttributeValueQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeValueHandlers
{
    public class GetAttributeValuesByAttributeIdQueryHandler : IRequestHandler<GetAttributeValuesByAttributeIdQuery, IEnumerable<AttributeValueDto>>
    {
        private readonly IAttributeValueRepository _valueRepository;
        private readonly IProductAttributeRepository _attributeRepository;

        public GetAttributeValuesByAttributeIdQueryHandler(
            IAttributeValueRepository valueRepository,
            IProductAttributeRepository attributeRepository)
        {
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<IEnumerable<AttributeValueDto>> Handle(GetAttributeValuesByAttributeIdQuery request, CancellationToken cancellationToken)
        {
            var attribute = await _attributeRepository.GetByIdAsync(request.AttributeId.ToString());
            if (attribute == null)
            {
                throw new ApplicationException($"Attribute with ID {request.AttributeId} not found");
            }

            var values = await _valueRepository.GetByAttributeIdAsync(request.AttributeId);

            return values.Select(v => new AttributeValueDto
            {
                Id = v.Id,
                AttributeId = v.AttributeId,
                ValueName = v.ValueName,
                CreatedAt = v.CreatedAt,
                CreatedBy = v.CreatedBy,
                LastModifiedAt = v.LastModifiedAt,
                LastModifiedBy = v.LastModifiedBy
            });
        }
    }
}