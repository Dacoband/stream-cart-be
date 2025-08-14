using LivestreamService.Application.DTOs;
using MediatR;
using System;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class PinProductCommand : IRequest<LivestreamProductDTO>
    {
        // Support both approaches for backward compatibility
        public Guid Id { get; set; } // For existing ID-based approach

        // New composite key approach
        public Guid LivestreamId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;

        public bool IsPin { get; set; }
        public Guid SellerId { get; set; }
    }
}