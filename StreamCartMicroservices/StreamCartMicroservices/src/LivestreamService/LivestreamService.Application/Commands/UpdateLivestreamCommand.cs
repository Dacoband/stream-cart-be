using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.Commands
{
    public class UpdateLivestreamCommand : IRequest<LivestreamDTO>
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public  string? ThumbnailUrl { get; set; }
        public string? Tags { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
