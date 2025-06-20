using MediatR;
using ProductService.Application.DTOs.Combinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Queries.CombinationQueries
{
    public class GetAllProductCombinationsQuery : IRequest<IEnumerable<ProductCombinationDto>>
    {
    }
}
