using Shared.Common.Domain.Bases;
using System;

namespace LivestreamService.Domain.Entities
{
    public class StreamView : BaseEntity
    {
        public Guid LivestreamId { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime? EndTime { get; private set; }

        // Private constructor for EF Core
        private StreamView() { }

        public StreamView(
            Guid livestreamId,
            Guid userId,
            DateTime startTime,
            string createdBy = "system")
        {
            LivestreamId = livestreamId;
            UserId = userId;
            StartTime = startTime;
            SetCreator(createdBy);
        }

        public void EndViewing(DateTime endTime, string modifiedBy)
        {
            EndTime = endTime;
            SetModifier(modifiedBy);
        }

        public override bool IsValid()
        {
            return LivestreamId != Guid.Empty &&
                   UserId != Guid.Empty;
        }
        // Thêm method này vào StreamView entity
        public void EndView(DateTime endTime, string modifiedBy)
        {
            EndTime = endTime;
            SetModifier(modifiedBy);
        }
    }
}