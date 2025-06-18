using MediatR;
using ProductService.Application.DTOs.Combinations;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Commands.CombinationCommands
{
    public class CreateProductCombinationCommand : IRequest<ProductCombinationDto>
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }
        public string? CreatedBy { get; set; }
    }
}