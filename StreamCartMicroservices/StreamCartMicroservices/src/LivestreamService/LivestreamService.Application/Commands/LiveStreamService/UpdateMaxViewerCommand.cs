using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class UpdateMaxViewerCommand : IRequest<LivestreamDTO>
    {
        public Guid LivestreamId { get; set; }
        public int MaxViewer { get; set; }
        public Guid SellerId { get; set; }
    }
}