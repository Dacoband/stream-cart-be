using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.Commands
{
    public class ApproveLivestreamContentCommand : IRequest<LivestreamDTO>
    {
        public Guid Id { get; set; }
        public bool Approved { get; set; }
        public Guid ModeratorId { get; set; }
    }
}
