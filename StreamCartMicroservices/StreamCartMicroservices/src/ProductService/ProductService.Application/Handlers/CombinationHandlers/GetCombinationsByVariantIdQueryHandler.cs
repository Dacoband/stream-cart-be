using MediatR;
using ProductService.Application.DTOs.Combinations;
using ProductService.Application.Queries.CombinationQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CombinationHandlers
{
    public class GetCombinationsByVariantIdQueryHandler : IRequestHandler<GetCombinationsByVariantIdQuery, IEnumerable<ProductCombinationDto>>
    {
        private readonly IProductCombinationRepository _combinationRepository;
        private readonly IAttributeValueRepository _valueRepository;
        private readonly IProductAttributeRepository _attributeRepository;

        public GetCombinationsByVariantIdQueryHandler(
            IProductCombinationRepository combinationRepository,
            IAttributeValueRepository valueRepository,
            IProductAttributeRepository attributeRepository)
        {
            _combinationRepository = combinationRepository ?? throw new ArgumentNullException(nameof(combinationRepository));
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<IEnumerable<ProductCombinationDto>> Handle(GetCombinationsByVariantIdQuery request, CancellationToken cancellationToken)
        {
            var combinations = await _combinationRepository.GetByVariantIdAsync(request.VariantId);

            var result = new List<ProductCombinationDto>();

            foreach (var combination in combinations)
            {
                var attributeValue = await _valueRepository.GetByIdAsync(combination.AttributeValueId.ToString());
                if (attributeValue == null) continue;

                var attribute = await _attributeRepository.GetByIdAsync(attributeValue.AttributeId.ToString());
                if (attribute == null) continue;

                result.Add(new ProductCombinationDto
                {
                    VariantId = combination.VariantId,
                    AttributeValueId = combination.AttributeValueId,
                    AttributeName = attribute.Name,
                    ValueName = attributeValue.ValueName,
                    CreatedAt = combination.CreatedAt,
                    CreatedBy = combination.CreatedBy,
                    LastModifiedAt = combination.LastModifiedAt,
                    LastModifiedBy = combination.LastModifiedBy
                });
            }

            return result;
        }
    }
}