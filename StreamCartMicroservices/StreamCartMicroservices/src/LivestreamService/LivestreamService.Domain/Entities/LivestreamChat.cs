using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Common.Domain.Bases;
using System;

namespace LivestreamService.Domain.Entities
{
    public class LivestreamChat
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("livestreamId")]
        [BsonRepresentation(BsonType.String)]
        public Guid LivestreamId { get; set; }

        [BsonElement("senderId")]
        [BsonRepresentation(BsonType.String)]
        public Guid SenderId { get; set; }

        [BsonElement("senderName")]
        public string SenderName { get; set; } = string.Empty;

        [BsonElement("senderType")]
        public string SenderType { get; set; } = string.Empty; // "Viewer", "Shop", "Moderator"

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("messageType")]
        public string MessageType { get; set; } = "Text"; // "Text", "Image", "Product", "Gift"

        [BsonElement("replyToMessageId")]
        [BsonRepresentation(BsonType.String)]
        public Guid? ReplyToMessageId { get; set; }

        [BsonElement("isModerated")]
        public bool IsModerated { get; set; } = false;

        [BsonElement("moderatedBy")]
        [BsonRepresentation(BsonType.String)]
        public Guid? ModeratedBy { get; set; }

        [BsonElement("moderatedAt")]
        public DateTime? ModeratedAt { get; set; }

        [BsonElement("sentAt")]
        public DateTime SentAt { get; set; }

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

        public LivestreamChat()
        {
            Id = Guid.NewGuid();
            SentAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }

        public LivestreamChat(
            Guid livestreamId,
            Guid senderId,
            string senderName,
            string senderType,
            string message,
            string messageType = "Text",
            Guid? replyToMessageId = null,
            string createdBy = "system") : this()
        {
            LivestreamId = livestreamId;
            SenderId = senderId;
            SenderName = senderName;
            SenderType = senderType;
            Message = message;
            MessageType = messageType;
            ReplyToMessageId = replyToMessageId;
            CreatedBy = createdBy;
        }

        public void Moderate(Guid moderatorId, string modifiedBy)
        {
            IsModerated = true;
            ModeratedBy = moderatorId;
            ModeratedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }

        public void Unmoderate(string modifiedBy)
        {
            IsModerated = false;
            ModeratedBy = null;
            ModeratedAt = null;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
    }
}