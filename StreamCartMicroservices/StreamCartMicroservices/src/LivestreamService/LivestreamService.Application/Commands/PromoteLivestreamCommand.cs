using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.Commands
{
    public class PromoteLivestreamCommand : IRequest<LivestreamDTO>
    {
        public Guid Id { get; set; }
        public bool IsPromoted { get; set; }
        public Guid AdminId { get; set; }
    }
}