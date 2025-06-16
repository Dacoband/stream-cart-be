using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;

namespace ProductService.Application.Commands.AttributeCommands
{
    public class CreateProductAttributeCommand : IRequest<ProductAttributeDto>
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }
    }

    public class UpdateProductAttributeCommand : IRequest<ProductAttributeDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class DeleteProductAttributeCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string DeletedBy { get; set; }
    }
}