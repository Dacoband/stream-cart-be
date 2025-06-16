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
    public class GetAllAttributeValuesQueryHandler : IRequestHandler<GetAllAttributeValuesQuery, IEnumerable<AttributeValueDto>>
    {
        private readonly IAttributeValueRepository _valueRepository;

        public GetAllAttributeValuesQueryHandler(IAttributeValueRepository valueRepository)
        {
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
        }

        public async Task<IEnumerable<AttributeValueDto>> Handle(GetAllAttributeValuesQuery request, CancellationToken cancellationToken)
        {
            var attributeValues = await _valueRepository.GetAllAsync();

            return attributeValues.Select(v => new AttributeValueDto
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