using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Commands.LiveStreamService
{
    public class UpdateStockCommand : IRequest<LivestreamProductDTO>
    {
        public Guid Id { get; set; }
        public int Stock { get; set; }
        public Guid SellerId { get; set; }
    }
}
