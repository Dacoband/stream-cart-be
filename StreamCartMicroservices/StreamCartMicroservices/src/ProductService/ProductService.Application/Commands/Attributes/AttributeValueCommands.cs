using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;

namespace ProductService.Application.Commands.AttributeValueCommands
{
    public class CreateAttributeValueCommand : IRequest<AttributeValueDto>
    {
        public Guid AttributeId { get; set; }
        public string ValueName { get; set; }
        public string CreatedBy { get; set; }
    }

    public class UpdateAttributeValueCommand : IRequest<AttributeValueDto>
    {
        public Guid Id { get; set; }
        public string ValueName { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class DeleteAttributeValueCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string DeletedBy { get; set; }
    }
}