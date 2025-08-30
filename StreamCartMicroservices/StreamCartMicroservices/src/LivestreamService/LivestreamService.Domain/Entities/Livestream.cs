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
        public Guid LivestreamHostId { get; private set; }
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

        public string? JoinToken { get; private set; }

        private Livestream() { }

        public Livestream(
            string title,
            string description,
            Guid sellerId,
            Guid shopId,
            Guid livestreamHostId,
            DateTime scheduledStartTime,
            string livekitRoomId,
            string streamKey = "",
            string thumbnailUrl = "",
            string tags = "",
            string? joinToken = null,
            string createdBy = "system")
        {
            Title = title;
            Description = description;
            SellerId = sellerId;
            ShopId = shopId;
            LivestreamHostId = livestreamHostId;
            ScheduledStartTime = scheduledStartTime;
            LivekitRoomId = livekitRoomId;
            StreamKey = streamKey;
            ThumbnailUrl = thumbnailUrl;
            Status = false;
            ApprovalStatusContent = false;
            IsPromoted = false;
            Tags = tags;
            JoinToken = joinToken;
            SetCreator(createdBy);
            SetModifier(createdBy);
        }

        public void UpdateDetails(string title, string description, DateTime scheduledStartTime,
             string thumbnailUrl, string tags, string modifiedBy, Guid requestingUserId)
        {
            // ✅ KIỂM TRA QUYỀN: Chỉ chủ shop (SellerId) mới được update
            if (requestingUserId != SellerId)
            {
                throw new UnauthorizedAccessException("Only the shop owner can update livestream details");
            }

            Title = title ?? throw new ArgumentNullException(nameof(title));
            Description = description ?? string.Empty;
            ScheduledStartTime = scheduledStartTime;
            ThumbnailUrl = thumbnailUrl ?? string.Empty;
            Tags = tags ?? string.Empty;
            SetModifier(modifiedBy);
        }

        public void SetJoinToken(string joinToken, string? modifiedBy = null)
        {
            JoinToken = joinToken;
            if (!string.IsNullOrEmpty(modifiedBy))
            {
                SetModifier(modifiedBy);
            }
        }
        public void Start(string modifiedBy, Guid requestingUserId)
        {
            if (requestingUserId != LivestreamHostId)
            {
                throw new UnauthorizedAccessException("Only the designated livestream host can start the livestream");
            }

            if (Status)
            {
                throw new InvalidOperationException("Livestream has already started");
            }

            Status = true;
            ActualStartTime = DateTime.UtcNow;
            SetModifier(modifiedBy);
        }
        //public void Start(string modifiedBy)
        //{
        //    ActualStartTime = DateTime.UtcNow;
        //    Status = true;
        //    SetModifier(modifiedBy);
        //}
        public void UpdateLivestreamHost(Guid newLivestreamHostId, string modifiedBy, Guid requestingUserId)
        {
            if (SellerId != requestingUserId)
                throw new UnauthorizedAccessException("Only the seller can update livestream host");
            LivestreamHostId = newLivestreamHostId;
            SetModifier(modifiedBy);
        }
        public void End(string modifiedBy)
        {
            if (!Status)
            {
                throw new InvalidOperationException("Livestream is not currently active");
            }

            Status = false;
            ActualEndTime = DateTime.UtcNow;
            SetModifier(modifiedBy);
        }
        public void SetPlaybackUrl(string playbackUrl)
        {
            PlaybackUrl = playbackUrl;
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
                   LivestreamHostId != Guid.Empty &&
                   !string.IsNullOrWhiteSpace(LivekitRoomId);
        }
    }
}