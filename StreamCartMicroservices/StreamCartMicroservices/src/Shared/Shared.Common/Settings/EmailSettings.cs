using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Settings
{  
    public class EmailSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string DefaultFromEmail { get; set; } = string.Empty;
        public string DefaultFromName { get; set; } = string.Empty;
        public string Provider { get; set; } = "MailJet"; 
    }
}
