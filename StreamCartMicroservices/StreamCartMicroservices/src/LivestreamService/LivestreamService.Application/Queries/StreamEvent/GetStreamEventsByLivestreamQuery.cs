using LivestreamService.Application.DTOs.StreamEvent;
using MediatR;
using System;
using System.Collections.Generic;

namespace LivestreamService.Application.Queries.StreamEvent
{
    public class GetStreamEventsByLivestreamQuery : IRequest<IEnumerable<StreamEventDTO>>
    {
        public Guid LivestreamId { get; set; }
        public int? Count { get; set; }
        public string? EventType { get; set; }
    }

}
