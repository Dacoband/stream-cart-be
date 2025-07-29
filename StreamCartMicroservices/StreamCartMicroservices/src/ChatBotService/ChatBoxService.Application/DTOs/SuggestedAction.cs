using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBoxService.Application.DTOs
{
    public class SuggestedAction
    {
        public string Title { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Url { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
