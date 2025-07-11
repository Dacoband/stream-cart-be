using Shared.Common.Domain.Bases;
using System;

namespace LivestreamService.Domain.Entities
{
    public class Livestream : BaseEntity
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public Guid SellerId { get; private set; }
        public Guid ShopId { get; private set; }
        public DateTime ScheduledStartTime { get; private set; }
        public DateTime? ActualStartTime { get; private set; }
        public DateTime? ActualEndTime { get; private set; }
        public bool Status { get; private set; }
        public string StreamKey { get; private set; }
        public string PlaybackUrl { get; private set; }
        public string ThumbnailUrl { get; private set; }
        public int? MaxViewer { get; private set; }
        public bool ApprovalStatusContent { get; private set; }
        public Guid? ApprovedByUserId { get; private set; }
        public DateTime? ApprovalDateContent { get; private set; }
        public bool IsPromoted { get; private set; }
        public string Tags { get; private set; }
        public string LivekitRoomId { get; private set; }

        // Private constructor for EF Core
        private Livestream() { }

        public Livestream(
            string title,
            string description,
            Guid sellerId,
            Guid shopId,
            DateTime scheduledStartTime,
            string livekitRoomId,
            string streamKey = "",
            string thumbnailUrl = "",
            string tags = "",
            string createdBy = "system")
        {
            Title = title;
            Description = description;
            SellerId = sellerId;
            ShopId = shopId;
            ScheduledStartTime = scheduledStartTime;
            LivekitRoomId = livekitRoomId;
            StreamKey = streamKey;
            ThumbnailUrl = thumbnailUrl;
            Status = false;
            ApprovalStatusContent = false;
            IsPromoted = false;
            Tags = tags;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }

        public void UpdateDetails(string title, string description, DateTime scheduledStartTime, string thumbnailUrl, string tags, string modifiedBy)
        {
            Title = title;
            Description = description;
            ScheduledStartTime = scheduledStartTime;
            ThumbnailUrl = thumbnailUrl;
            Tags = tags;
            SetModifier(modifiedBy);
        }

        public void Start(string modifiedBy)
        {
            ActualStartTime = DateTime.UtcNow;
            Status = true;
            SetModifier(modifiedBy);
        }

        public void End(string modifiedBy)
        {
            ActualEndTime = DateTime.UtcNow;
            Status = false;
            SetModifier(modifiedBy);
        }

        public void SetPlaybackUrl(string url, string modifiedBy)
        {
            PlaybackUrl = url;
            SetModifier(modifiedBy);
        }

        public void SetMaxViewer(int maxViewer, string modifiedBy)
        {
            MaxViewer = maxViewer;
            SetModifier(modifiedBy);
        }

        public void ApproveContent(Guid approvedByUserId, string modifiedBy)
        {
            ApprovalStatusContent = true;
            ApprovedByUserId = approvedByUserId;
            ApprovalDateContent = DateTime.UtcNow;
            SetModifier(modifiedBy);
        }

        public void SetPromotion(bool isPromoted, string modifiedBy)
        {
            IsPromoted = isPromoted;
            SetModifier(modifiedBy);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Title) &&
                   SellerId != Guid.Empty &&
                   ShopId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(LivekitRoomId);
        }
    }
}