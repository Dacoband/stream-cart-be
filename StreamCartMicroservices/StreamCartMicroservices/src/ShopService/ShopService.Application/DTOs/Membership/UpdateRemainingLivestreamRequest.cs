using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Membership
{
    public class UpdateRemainingLivestreamRequest
    {
        public int RemainingLivestream { get; set; }
    }

    public class CreateShopMembershipRequest
    {
        public string MembershipId { get; set; } = string.Empty;
    }

    public class ShopMembershipDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int RemainingLivestream { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MaxProduct { get; set; }
        public decimal Commission { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive => Status == "Ongoing" &&
                               DateTime.UtcNow >= StartDate &&
                              // DateTime.UtcNow <= EndDate &&
                               !IsDeleted;

        public bool HasRemainingLivestreamTime => RemainingLivestream > 0;
        public int RemainingLivestreamMinutes => RemainingLivestream;
    }
}
