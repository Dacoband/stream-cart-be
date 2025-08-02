using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.DTOs.Chat
{
    public class LivestreamChatDTO1
    {
        public Guid LivestreamId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public long Timestamp { get; set; }
    }
}
