using MediatR;
using ProductService.Application.Commands.AttributeCommands;
using ProductService.Application.DTOs.Attributes;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeHandlers
{
    public class CreateProductAttributeCommandHandler : IRequestHandler<CreateProductAttributeCommand, ProductAttributeDto>
    {
        private readonly IProductAttributeRepository _attributeRepository;

        public CreateProductAttributeCommandHandler(IProductAttributeRepository attributeRepository)
        {
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<ProductAttributeDto> Handle(CreateProductAttributeCommand request, CancellationToken cancellationToken)
        {
            // Validate input to ensure 'Name' is not null or empty
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("The attribute name cannot be null or empty.", nameof(request.Name));
            }

            // Validate 'CreatedBy' to ensure it is not null or empty
            if (string.IsNullOrWhiteSpace(request.CreatedBy))
            {
                throw new ArgumentException("The 'CreatedBy' field cannot be null or empty.", nameof(request.CreatedBy));
            }

            // Check if name is unique
            if (!await _attributeRepository.IsNameUniqueAsync(request.Name))
            {
                throw new ApplicationException($"An attribute with the name '{request.Name}' already exists");
            }

            // Create the attribute
            var attribute = new ProductAttribute(request.Name, request.CreatedBy);

            // Save to database
            await _attributeRepository.InsertAsync(attribute);

            // Return DTO
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