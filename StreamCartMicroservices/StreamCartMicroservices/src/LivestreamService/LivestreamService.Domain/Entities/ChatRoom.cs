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
        [BsonElement("liveKitRoomName")]
        public string? LiveKitRoomName { get; set; }

        [BsonElement("customerToken")]
        public string? CustomerToken { get; set; }
        [BsonElement("userName")]
        public string? UserName { get; set; }

        [BsonElement("shopName")]
        public string? ShopName { get; set; }

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
            string? liveKitRoomName = null,
            string? customerToken = null,
            string? userName = null,
            string? shopName = null,
            Guid? relatedOrderId = null,
            string createdBy = "system") : this()
        {
            UserId = userId;
            ShopId = shopId;
            LiveKitRoomName = liveKitRoomName;
            CustomerToken = customerToken;
            UserName = userName;
            ShopName = shopName;
            RelatedOrderId = relatedOrderId;
            CreatedBy = createdBy;
        }


        public void UpdateLastMessageTime(DateTime messageTime, string modifiedBy)
        {
            LastMessageAt = messageTime;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }
        public void UpdateLiveKitInfo(string? liveKitRoomName, string? customerToken, string modifiedBy)
        {
            LiveKitRoomName = liveKitRoomName;
            CustomerToken = customerToken;
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
        public void UpdateUserInfo(string? userName, string modifiedBy)
        {
            UserName = userName;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }

        public void UpdateShopInfo(string? shopName, string modifiedBy)
        {
            ShopName = shopName;
            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy;
        }

        public void UpdateChatRoomInfo(
            string? liveKitRoomName = null,
            string? customerToken = null,
            string? userName = null,
            string? shopName = null,
            string? modifiedBy = null)
        {
            if (!string.IsNullOrEmpty(liveKitRoomName))
                LiveKitRoomName = liveKitRoomName;

            if (!string.IsNullOrEmpty(customerToken))
                CustomerToken = customerToken;

            if (!string.IsNullOrEmpty(userName))
                UserName = userName;

            if (!string.IsNullOrEmpty(shopName))
                ShopName = shopName;

            LastModifiedAt = DateTime.UtcNow;
            LastModifiedBy = modifiedBy ?? "system";
        }
    }
}