﻿using MediatR;
using ProductService.Application.DTOs.Combinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.CombinationQueries
{
    public class GetCombinationsByVariantIdQuery : IRequest<IEnumerable<ProductCombinationDto>>
    {
        public Guid VariantId { get; set; }
    }
}
