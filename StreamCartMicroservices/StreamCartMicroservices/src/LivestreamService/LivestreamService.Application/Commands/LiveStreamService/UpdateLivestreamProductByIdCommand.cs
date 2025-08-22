using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class UpdateLivestreamProductByIdCommand : IRequest<LivestreamProductDTO>
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsPin { get; set; }
        public Guid SellerId { get; set; }
    }
}