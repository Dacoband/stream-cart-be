using MediatR;
using ProductService.Application.DTOs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.AttributeCommands
{
    public class UpdateProductAttributeCommand : IRequest<ProductAttributeDto>
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
