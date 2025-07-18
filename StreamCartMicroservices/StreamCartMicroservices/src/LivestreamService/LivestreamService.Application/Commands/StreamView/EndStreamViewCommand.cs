using LivestreamService.Application.DTOs.StreamView;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.StreamView
{
    public class EndStreamViewCommand : IRequest<StreamViewDTO>
    {
        public Guid StreamViewId { get; set; }
        public Guid UserId { get; set; }
    }
}