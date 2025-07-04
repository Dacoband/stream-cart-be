using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.DTOs
{
    public class DetailNotificationDTO
    {
        public Guid NotificationId { get; set; }
        public string RecipentUserId { get; set; }
        public string? OrderCode { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public Guid? LivestreamId { get; set; }
        public string Type { get; set; }    
        public string Message { get; set; }
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime? Created { get; set; }
    }
    public class ListNotificationDTO
    {
        public int TotalItem { get; set; }
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public List<DetailNotificationDTO> NotificationList { get; set; }
    }
}
