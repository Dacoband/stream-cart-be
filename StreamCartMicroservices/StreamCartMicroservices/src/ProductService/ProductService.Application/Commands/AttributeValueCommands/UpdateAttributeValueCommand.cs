using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.AttributeValueCommands
{
    public class UpdateAttributeValueCommand : IRequest<AttributeValueDto>
    {
        public Guid Id { get; set; }
        public string? ValueName { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
