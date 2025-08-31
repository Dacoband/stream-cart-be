using System;

namespace LivestreamService.Application.DTOs
{
    public class ShopMembershipDto
    {
        public Guid Id { get; set; }
        public Guid MembershipId { get; set; }
        public Guid ShopId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RemainingLivestream { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? MaxProduct { get; set; }
        public decimal? Commission { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string MembershipName { get; set; } = string.Empty;
        public string MembershipType { get; set; } = string.Empty;
        public int MaxLivestream { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public bool IsActive => Status == "Ongoing" &&
                               DateTime.UtcNow >= StartDate &&
                               DateTime.UtcNow <= EndDate &&
                               !IsDeleted;
        public bool HasRemainingLivestreamTime => RemainingLivestream > 0;
        public int RemainingLivestreamMinutes => RemainingLivestream;
    }
}