using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class UpdateStockByIdCommand : IRequest<LivestreamProductDTO>
    {
        public Guid Id { get; set; }
        public int Stock { get; set; }
        public Guid SellerId { get; set; }
    }
}