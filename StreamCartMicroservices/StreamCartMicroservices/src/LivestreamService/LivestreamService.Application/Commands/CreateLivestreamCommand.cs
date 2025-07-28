using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Commands
{
    public class CreateLivestreamCommand : IRequest<LivestreamDTO>
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Guid ShopId { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Tags { get; set; }
        public Guid SellerId { get; set; }
        public List<CreateLivestreamProductItemDTO>? Products { get; set; } = new();

    }
}