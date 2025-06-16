using MediatR;
using ProductService.Application.Commands.AttributeCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeHandlers
{
    public class UpdateProductAttributeCommandHandler : IRequestHandler<UpdateProductAttributeCommand, ProductAttributeDto>
    {
        private readonly IProductAttributeRepository _attributeRepository;

        public UpdateProductAttributeCommandHandler(IProductAttributeRepository attributeRepository)
        {
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<ProductAttributeDto> Handle(UpdateProductAttributeCommand request, CancellationToken cancellationToken)
        {
            var attribute = await _attributeRepository.GetByIdAsync(request.Id.ToString());
            if (attribute == null)
            {
                throw new ApplicationException($"Product attribute with ID {request.Id} not found");
            }

            // Check if name is unique if changed
            if (attribute.Name != request.Name && !await _attributeRepository.IsNameUniqueAsync(request.Name, request.Id))
            {
                throw new ApplicationException($"An attribute with the name '{request.Name}' already exists");
            }

            attribute.UpdateName(request.Name);

            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                attribute.SetUpdatedBy(request.UpdatedBy);
            }

            await _attributeRepository.ReplaceAsync(attribute.Id.ToString(), attribute);

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