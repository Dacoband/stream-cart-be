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
        public string CreatedBy { get; set; }
    }

    public class UpdateProductCombinationCommand : IRequest<ProductCombinationDto>
    {
        public Guid CurrentVariantId { get; set; }
        public Guid CurrentAttributeValueId { get; set; }
        public Guid NewAttributeValueId { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class DeleteProductCombinationCommand : IRequest<bool>
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }
        public string DeletedBy { get; set; }
    }

    public class GenerateProductCombinationsCommand : IRequest<bool>
    {
        public Guid ProductId { get; set; }
        public List<AttributeValueGroup> AttributeValueGroups { get; set; } = new List<AttributeValueGroup>();
        public decimal DefaultPrice { get; set; }
        public int DefaultStock { get; set; }
        public string CreatedBy { get; set; }
    }
}