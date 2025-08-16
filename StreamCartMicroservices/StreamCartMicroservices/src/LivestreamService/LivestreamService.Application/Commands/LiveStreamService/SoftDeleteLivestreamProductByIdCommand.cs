using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class SoftDeleteLivestreamProductByIdCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid SellerId { get; set; }
    }
}