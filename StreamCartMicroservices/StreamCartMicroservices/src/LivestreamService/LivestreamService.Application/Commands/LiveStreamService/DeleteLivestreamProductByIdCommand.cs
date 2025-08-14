using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class DeleteLivestreamProductByIdCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
    }
}