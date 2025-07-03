using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notification.Domain.Entities
{
    public class Notifications : BaseEntity
    {
        [BsonRepresentation(BsonType.String)]
        public string RecipientUserID { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string OrderCode { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? ProductId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? VariantId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid? LivestreamId { get; set; }

        [BsonElement("Type")]
        public string Type { get; set; }

        public string Message { get; set; }

        public string? LinkUrl { get; set; }

        public bool IsRead { get; set; } = false;
    }
}
