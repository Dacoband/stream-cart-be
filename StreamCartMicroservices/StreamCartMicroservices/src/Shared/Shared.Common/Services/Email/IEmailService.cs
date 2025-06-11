using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Services.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? toName = null);
        Task<bool> SendEmailAsync(string[] to, string subject, string htmlBody, string[]? toNames = null);
        Task<bool> SendTemplateEmailAsync(string to, string templateId, object templateData, string? toName = null);
    }
}
