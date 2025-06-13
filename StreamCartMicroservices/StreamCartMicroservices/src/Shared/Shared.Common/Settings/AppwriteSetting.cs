using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Shared.Common.Settings
{
    public class AppwriteSetting
    {
        public string ProjectID { get; set; } = string.Empty;
        public string Endpoint {  get; set; } = string.Empty;
        public string BucketID { get; set; } = string.Empty;
        public string APIKey { get; set; } = string.Empty;

    }
}
