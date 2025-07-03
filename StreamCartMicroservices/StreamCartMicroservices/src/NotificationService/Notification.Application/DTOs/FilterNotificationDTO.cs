using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.DTOs
{
    public class FilterNotificationDTO
    {
        public string? Type { get; set; }
        public bool? IsRead { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
