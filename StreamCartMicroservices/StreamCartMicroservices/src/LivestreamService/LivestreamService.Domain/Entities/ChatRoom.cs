using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LivestreamService.Domain.Entities
{
    public class ChatRoom
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonElement("shopId")]
        [BsonRepresentation(BsonType.String)]
        public Guid ShopId { get; set; }

        [BsonElement("startedAt")]
        public DateTime StartedAt { get; set; }

        [BsonElement("lastMessageAt")]
        public DateTime? LastMessageAt { get; set; }

        [BsonElement("relatedOrderId")]
        [BsonRepresentation(BsonType.String)]
        public Guid? RelatedOrderId { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("createdBy")]
        public string CreatedBy { get; set; } = string.Empty;

        [BsonElement("lastModifiedAt")]
        public DateTime? LastModifiedAt { get; set; }

        [BsonElement("lastModifiedBy")]
        public string? LastModifiedBy { get; set; }

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; } = false;

        public ChatRoom()
        {
            Id = Guid.NewGuid();
            StartedAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }

        public ChatRoom(
            Guid userId,
            Guid shopId,
            Guid? relatedOrderId = null,
            string createdBy = "system") : this()
        {
            UserId = userId;
            ShopId = shopId;
            RelatedOrderId = relatedOrderId;
            CreatedBy = createdBy;
        }

        public void UpdateLastMessageTime(DateTime messageTime, string modifiedBy)
        {
            LastMessageAt = messageTime;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }

        public void DeactivateRoom(string modifiedBy)
        {
            IsActive = false;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }

        public void ReactivateRoom(string modifiedBy)
        {
            IsActive = true;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
    }
}