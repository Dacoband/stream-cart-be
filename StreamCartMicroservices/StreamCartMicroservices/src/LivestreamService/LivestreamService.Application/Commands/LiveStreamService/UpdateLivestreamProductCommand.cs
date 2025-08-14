using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class UpdateLivestreamProductCommand : IRequest<LivestreamProductDTO>
    {
        public Guid LivestreamId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public bool? IsPin { get; set; }
        public Guid SellerId { get; set; }
    }
}
