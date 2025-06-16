using MediatR;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Queries.AttributeQueries;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeHandlers
{
    public class GetProductAttributeByIdQueryHandler : IRequestHandler<GetProductAttributeByIdQuery, ProductAttributeDto>
    {
        private readonly IProductAttributeRepository _attributeRepository;

        public GetProductAttributeByIdQueryHandler(IProductAttributeRepository attributeRepository)
        {
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<ProductAttributeDto> Handle(GetProductAttributeByIdQuery request, CancellationToken cancellationToken)
        {
            var attribute = await _attributeRepository.GetByIdAsync(request.Id.ToString());
            if (attribute == null)
            {
                return null;
            }

            return new ProductAttributeDto
            {
                Id = attribute.Id,
                Name = attribute.Name,
                CreatedAt = attribute.CreatedAt,
                CreatedBy = attribute.CreatedBy,
                LastModifiedAt = attribute.LastModifiedAt,
                LastModifiedBy = attribute.LastModifiedBy
            };
        }
    }
}