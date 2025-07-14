using LivestreamService.Application.DTOs;
using MediatR;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.Queries
{
    public class GetUpcomingLivestreamsQuery : IRequest<List<LivestreamDTO>>
    {
        public PaginationParams Pagination { get; set; } = new PaginationParams { PageNumber = 1, PageSize = 10 };
        public bool IncludePromotedOnly { get; set; }
    }
}
