using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LivestreamService.Domain.Entities
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("chatRoomId")]
        [BsonRepresentation(BsonType.String)]
        public Guid ChatRoomId { get; set; }

        [BsonElement("senderUserId")]
        [BsonRepresentation(BsonType.String)]
        public Guid SenderUserId { get; set; }

        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;

        [BsonElement("sentAt")]
        public DateTime SentAt { get; set; }

        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;

        [BsonElement("readAt")]
        public DateTime? ReadAt { get; set; }

        [BsonElement("isEdited")]
        public bool IsEdited { get; set; } = false;

        [BsonElement("editedAt")]
        public DateTime? EditedAt { get; set; }

        [BsonElement("messageType")]
        public string MessageType { get; set; } = "Text"; // "Text", "Image", "File", "System"

        [BsonElement("attachmentUrl")]
        public string? AttachmentUrl { get; set; }

        [BsonElement("replyToMessageId")]
        [BsonRepresentation(BsonType.String)]
        public Guid? ReplyToMessageId { get; set; }

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

        public ChatMessage()
        {
            Id = Guid.NewGuid();
            SentAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
        }

        public ChatMessage(
            Guid chatRoomId,
            Guid senderUserId,
            string content,
            string messageType = "Text",
            string? attachmentUrl = null,
            string createdBy = "system") : this()
        {
            ChatRoomId = chatRoomId;
            SenderUserId = senderUserId;
            Content = content;
            MessageType = messageType;
            AttachmentUrl = attachmentUrl;
            CreatedBy = createdBy;
        }

        public void MarkAsRead(string modifiedBy)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }

        public void EditContent(string newContent, string modifiedBy)
        {
            Content = newContent;
            IsEdited = true;
            EditedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
        public void EditMessage(string newContent, string modifiedBy)
        {
            Content = newContent;
            IsEdited = true;
            EditedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
    }
}