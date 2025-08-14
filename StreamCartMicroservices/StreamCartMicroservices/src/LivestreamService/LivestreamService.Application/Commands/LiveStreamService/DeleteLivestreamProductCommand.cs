using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class DeleteLivestreamProductCommand : IRequest<bool>
    {
        public Guid LivestreamId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string VariantId { get; set; } = string.Empty;
        public Guid SellerId { get; set; }
    }
}
