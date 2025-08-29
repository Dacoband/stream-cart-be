using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Application.DTOs
{
    public class CreateNotificationDTO 
    {
     
        public string RecipientUserID { get; set; }

        public string? OrderCode { get; set; }

        public Guid? ProductId { get; set; }

        public Guid? VariantId { get; set; }

        public Guid? LivestreamId { get; set; }
        public string Message { get; set; }

    }
}
