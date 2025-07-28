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
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
    }
}
