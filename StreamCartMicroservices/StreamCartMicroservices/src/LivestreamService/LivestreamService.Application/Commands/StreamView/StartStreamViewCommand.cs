using LivestreamService.Application.DTOs.StreamView;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.StreamView
{
    public class StartStreamViewCommand : IRequest<StreamViewDTO>
    {
        public Guid LivestreamId { get; set; }
        public Guid UserId { get; set; }
    }
}