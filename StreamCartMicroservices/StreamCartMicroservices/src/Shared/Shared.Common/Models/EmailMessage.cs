using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Models
{
    public class EmailMessage
    {
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public List<EmailRecipient> To { get; set; } = new List<EmailRecipient>();
        public string Subject { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public string? TextBody { get; set; }
        public Dictionary<string, object>? TemplateData { get; set; }
        public string? TemplateId { get; set; }
    }

    public class EmailRecipient
    {
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }

        public EmailRecipient(string email, string? name = null)
        {
            Email = email;
            Name = name;
        }
    }
}
