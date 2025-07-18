using LivestreamService.Application.DTOs.StreamView;
using MediatR;
using System;

namespace LivestreamService.Application.Queries.StreamView
{
    public class GetStreamViewStatsQuery : IRequest<StreamViewStatsDTO>
    {
        public Guid LivestreamId { get; set; }
    }
}