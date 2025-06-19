using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;

namespace ProductService.Application.Commands.AttributeValueCommands
{
    public class CreateAttributeValueCommand : IRequest<AttributeValueDto>
    {
        public Guid AttributeId { get; set; }
        public string? ValueName { get; set; }
        public string? CreatedBy { get; set; }
    }
}