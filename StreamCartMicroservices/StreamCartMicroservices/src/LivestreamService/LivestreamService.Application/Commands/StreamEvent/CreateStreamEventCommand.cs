using LivestreamService.Application.DTOs.StreamEvent;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.StreamEvent
{
    public class CreateStreamEventCommand : IRequest<StreamEventDTO>
    {
        public Guid LivestreamId { get; set; }
        public Guid UserId { get; set; }
        public Guid? LivestreamProductId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}