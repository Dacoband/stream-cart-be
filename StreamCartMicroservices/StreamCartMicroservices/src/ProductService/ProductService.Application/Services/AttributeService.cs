using MediatR;
using ProductService.Application.Commands.AttributeCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.AttributeQueries;
using ProductService.Application.Queries.AttributeValueQueries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class AttributeService : IProductAttributeService
    {
        private readonly IMediator _mediator;

        public AttributeService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<ProductAttributeDto>> GetAllAsync()
        {
            return await _mediator.Send(new GetAllProductAttributesQuery());
        }

        public async Task<ProductAttributeDto?> GetByIdAsync(Guid id)
        {
            return await _mediator.Send(new GetProductAttributeByIdQuery { Id = id });
        }

        public async Task<ProductAttributeDto> CreateAsync(CreateProductAttributeDto dto, string createdBy)
        {
            var command = new CreateProductAttributeCommand
            {
                Name = dto.Name ?? string.Empty,
                CreatedBy = createdBy
            };
            return await _mediator.Send(command);
        }

        public async Task<ProductAttributeDto> UpdateAsync(Guid id, UpdateProductAttributeDto dto, string updatedBy)
        {
            var command = new UpdateProductAttributeCommand
            {
                Id = id,
                Name = dto.Name,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteAsync(Guid id, string deletedBy)
        {
            var command = new DeleteProductAttributeCommand
            {
                Id = id,
                DeletedBy = deletedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<IEnumerable<ProductAttributeDto>> GetByProductIdAsync(Guid productId)
        {
            return await _mediator.Send(new GetAttributesByProductIdQuery { ProductId = productId });
        }

        public async Task<IEnumerable<AttributeValueDto>> GetValuesByAttributeIdAsync(Guid attributeId)
        {
            return await _mediator.Send(new GetAttributeValuesByAttributeIdQuery { AttributeId = attributeId });
        }
    }
}