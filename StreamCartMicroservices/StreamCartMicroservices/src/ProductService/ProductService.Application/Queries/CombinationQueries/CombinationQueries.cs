using MediatR;
using ProductService.Application.DTOs.Combinations;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Queries.CombinationQueries
{
    public class GetAllProductCombinationsQuery : IRequest<IEnumerable<ProductCombinationDto>>
    {
    }

    public class GetCombinationsByVariantIdQuery : IRequest<IEnumerable<ProductCombinationDto>>
    {
        public Guid VariantId { get; set; }
    }

    public class GetCombinationsByProductIdQuery : IRequest<IEnumerable<ProductCombinationDto>>
    {
        public Guid ProductId { get; set; }
    }
}