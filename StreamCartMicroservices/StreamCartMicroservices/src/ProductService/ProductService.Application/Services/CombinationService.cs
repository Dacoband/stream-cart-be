using MediatR;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Application.DTOs.Combinations;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.CombinationQueries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class CombinationService : IProductCombinationService
    {
        private readonly IMediator _mediator;

        public CombinationService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<ProductCombinationDto>> GetAllAsync()
        {
            return await _mediator.Send(new GetAllProductCombinationsQuery());
        }

        public async Task<IEnumerable<ProductCombinationDto>> GetByVariantIdAsync(Guid variantId)
        {
            return await _mediator.Send(new GetCombinationsByVariantIdQuery { VariantId = variantId });
        }

        public async Task<ProductCombinationDto> CreateAsync(CreateProductCombinationDto dto, string createdBy)
        {
            var command = new CreateProductCombinationCommand
            {
                VariantId = dto.VariantId,
                AttributeValueId = dto.AttributeValueId,
                CreatedBy = createdBy
            };
            return await _mediator.Send(command);
        }

        public async Task<ProductCombinationDto> UpdateAsync(Guid variantId, Guid attributeValueId, UpdateProductCombinationDto dto, string updatedBy)
        {
            var command = new UpdateProductCombinationCommand
            {
                CurrentVariantId = variantId,
                CurrentAttributeValueId = attributeValueId,
                NewAttributeValueId = dto.AttributeValueId,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteAsync(Guid variantId, Guid attributeValueId, string deletedBy)
        {
            var command = new DeleteProductCombinationCommand
            {
                VariantId = variantId,
                AttributeValueId = attributeValueId,
                DeletedBy = deletedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<IEnumerable<ProductCombinationDto>> GetByProductIdAsync(Guid productId)
        {
            return await _mediator.Send(new GetCombinationsByProductIdQuery { ProductId = productId });
        }

        public async Task<bool> GenerateCombinationsAsync(Guid productId, List<AttributeValueGroup> attributeValueGroups, decimal defaultPrice, int defaultStock, string createdBy)
        {
            var command = new GenerateProductCombinationsCommand
            {
                ProductId = productId,
                AttributeValueGroups = attributeValueGroups,
                DefaultPrice = defaultPrice,
                DefaultStock = defaultStock,
                CreatedBy = createdBy
            };
            return await _mediator.Send(command);
        }
    }
}