using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBoxService.Application.DTOs
{
    public class ChatbotIntent
    {
        public string Intent { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new();
        public bool RequiresProductInfo { get; set; }
        public bool RequiresShopInfo { get; set; }
        public decimal Confidence { get; set; }
    }
}
