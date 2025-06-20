using MediatR;
using ProductService.Application.Commands.AttributeValueCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.AttributeValueQueries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class AttributeValueService : IAttributeValueService
    {
        private readonly IMediator _mediator;

        public AttributeValueService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<AttributeValueDto>> GetAllAsync()
        {
            return await _mediator.Send(new GetAllAttributeValuesQuery());
        }

        public async Task<AttributeValueDto?> GetByIdAsync(Guid id)
        {
            return await _mediator.Send(new GetAttributeValueByIdQuery { Id = id });
        }

        public async Task<AttributeValueDto> CreateAsync(CreateAttributeValueDto dto, string createdBy)
        {
            var command = new CreateAttributeValueCommand
            {
                AttributeId = dto.AttributeId,
                ValueName = dto.ValueName!,
                CreatedBy = createdBy
            };
            return await _mediator.Send(command);
        }

        public async Task<AttributeValueDto> UpdateAsync(Guid id, UpdateAttributeValueDto dto, string updatedBy)
        {
            var command = new UpdateAttributeValueCommand
            {
                Id = id,
                ValueName = dto.ValueName!,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteAsync(Guid id, string deletedBy)
        {
            var command = new DeleteAttributeValueCommand
            {
                Id = id,
                DeletedBy = deletedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<IEnumerable<AttributeValueDto>> GetByAttributeIdAsync(Guid attributeId)
        {
            return await _mediator.Send(new GetAttributeValuesByAttributeIdQuery { AttributeId = attributeId });
        }
    }
}