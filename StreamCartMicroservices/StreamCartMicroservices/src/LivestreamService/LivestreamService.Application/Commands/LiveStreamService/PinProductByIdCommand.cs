using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class PinProductByIdCommand : IRequest<LivestreamProductDTO>
    {
        public Guid Id { get; set; }
        public bool IsPin { get; set; }
        public Guid SellerId { get; set; }
    }
}