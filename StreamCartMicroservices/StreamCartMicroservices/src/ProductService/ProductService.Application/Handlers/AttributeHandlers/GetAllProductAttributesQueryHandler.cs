using MediatR;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Queries.AttributeQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeHandlers
{
    public class GetAllProductAttributesQueryHandler : IRequestHandler<GetAllProductAttributesQuery, IEnumerable<ProductAttributeDto>>
    {
        private readonly IProductAttributeRepository _attributeRepository;

        public GetAllProductAttributesQueryHandler(IProductAttributeRepository attributeRepository)
        {
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<IEnumerable<ProductAttributeDto>> Handle(GetAllProductAttributesQuery request, CancellationToken cancellationToken)
        {
            var attributes = await _attributeRepository.GetAllAsync();

            return attributes.Select(attr => new ProductAttributeDto
            {
                Id = attr.Id,
                Name = attr.Name,
                CreatedAt = attr.CreatedAt,
                CreatedBy = attr.CreatedBy,
                LastModifiedAt = attr.LastModifiedAt,
                LastModifiedBy = attr.LastModifiedBy 
            });
        }
    }
}