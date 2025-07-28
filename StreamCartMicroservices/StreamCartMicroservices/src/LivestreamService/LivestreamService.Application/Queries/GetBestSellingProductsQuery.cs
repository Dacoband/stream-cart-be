using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Queries
{
    public class GetBestSellingProductsQuery : IRequest<IEnumerable<LivestreamProductSummaryDTO>>
    {
        public Guid LivestreamId { get; set; }
        public int Limit { get; set; } = 10;
    }
}
