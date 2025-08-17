using Shared.Common.Domain.Bases;
using Shared.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Domain.Entities
{
    public class LivestreamCart : BaseEntity
    {
        [Required]
        public Guid LivestreamId { get; set; }

        [Required]
        public Guid ViewerId { get; set; }

        public virtual ICollection<LivestreamCartItem> Items { get; set; } = new List<LivestreamCartItem>();

        // Metadata
        public DateTime? ExpiresAt { get; set; } // Cart expires when livestream ends
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Livestream? Livestream { get; set; }

        private LivestreamCart() { }

        public LivestreamCart(Guid livestreamId, Guid viewerId, string createdBy = "system")
        {
            LivestreamId = livestreamId;
            ViewerId = viewerId;
            IsActive = true;
            SetCreator(createdBy);
        }

        public void SetExpiration(DateTime expiresAt, string modifiedBy)
        {
            ExpiresAt = expiresAt;
            SetModifier(modifiedBy);
        }

        public void Deactivate(string modifiedBy)
        {
            IsActive = false;
            SetModifier(modifiedBy);
        }

        public override bool IsValid()
        {
            return LivestreamId != Guid.Empty && ViewerId != Guid.Empty;
        }
    }
}