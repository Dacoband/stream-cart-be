using Shared.Common.Domain.Bases;
using System;

namespace LivestreamService.Domain.Entities
{
    public class StreamEvent : BaseEntity
    {
        public Guid LivestreamId { get; private set; }
        public Guid UserId { get; private set; }
        public Guid? LivestreamProductId { get; private set; }
        public string EventType { get; private set; }
        public string Payload { get; private set; }

        // Private constructor for EF Core
        private StreamEvent() { }

        public StreamEvent(
            Guid livestreamId,
            Guid userId,
            string eventType,
            string payload,
            Guid? livestreamProductId = null,
            string createdBy = "system")
        {
            LivestreamId = livestreamId;
            UserId = userId;
            EventType = eventType;
            Payload = payload;
            LivestreamProductId = livestreamProductId;
            SetCreator(createdBy);
        }

        public override bool IsValid()
        {
            return LivestreamId != Guid.Empty &&
                   UserId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(EventType);
        }

    }
}