using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class UpdateDisplayOrderCommand : IRequest<LivestreamProductDTO>
    {
        public Guid Id { get; set; }
        //public int DisplayOrder { get; set; }
        public Guid SellerId { get; set; }
    }
}