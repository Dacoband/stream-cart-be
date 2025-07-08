using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.Commands
{
    public class EndLivestreamCommand : IRequest<LivestreamDTO>
    {
        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
    }
}
