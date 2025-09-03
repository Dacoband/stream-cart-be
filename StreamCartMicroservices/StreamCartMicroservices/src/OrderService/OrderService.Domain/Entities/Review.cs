using Shared.Common.Domain.Bases;
using ReviewService.Domain.Enums;
using System;
using System.Collections.Generic;

namespace OrderService.Domain.Entities
{
    public class Review : BaseEntity
    {
        public Guid? OrderID { get; private set; }
        public Guid? ProductID { get; private set; }
        public Guid? LivestreamId { get; private set; }
        public Guid AccountID { get; private set; }
        public int Rating { get; private set; } // 1-5 stars
        public string ReviewText { get; private set; } = string.Empty;
        public bool IsVerifiedPurchase { get; private set; }
        public ReviewType Type { get; private set; }
        public List<string> ImageUrls { get; private set; } = new List<string>();
        public DateTime? ApprovedAt { get; private set; }
        public string? ApprovedBy { get; private set; }
        public int HelpfulCount { get; private set; }
        public int UnhelpfulCount { get; private set; }

        protected Review() : base() { }

        public Review(
            Guid? orderId,
            Guid? productId,
            Guid? livestreamId,
            Guid accountId,
            int rating,
            string reviewText,
            ReviewType type,
            bool isVerifiedPurchase = false,
            List<string>? imageUrls = null, 
            string? createdBy = null) : base()
        {
            ValidateRating(rating);

            OrderID = orderId;
            ProductID = productId;
            LivestreamId = livestreamId;
            AccountID = accountId;
            Rating = rating;
            ReviewText = reviewText ?? string.Empty;
            Type = type;
            IsVerifiedPurchase = isVerifiedPurchase;
            ImageUrls = imageUrls ?? new List<string>(); 
            HelpfulCount = 0;
            UnhelpfulCount = 0;

            if (!string.IsNullOrEmpty(createdBy))
                SetCreator(createdBy);
        }

        public void UpdateReview(string reviewText, int rating, List<string>? imageUrls = null, string? modifiedBy = null)
        {
            ValidateRating(rating);

            ReviewText = reviewText ?? ReviewText;
            Rating = rating;

            if (imageUrls != null)
                ImageUrls = imageUrls; // ✅ CHANGED: Assign list of URLs

            if (!string.IsNullOrEmpty(modifiedBy))
                SetModifier(modifiedBy);
            else
                LastModifiedAt = DateTime.UtcNow;
        }

        public void MarkAsHelpful()
        {
            HelpfulCount++;
            LastModifiedAt = DateTime.UtcNow;
        }

        public void MarkAsUnhelpful()
        {
            UnhelpfulCount++;
            LastModifiedAt = DateTime.UtcNow;
        }

        private static void ValidateReviewTarget(Guid? orderId, Guid? productId, Guid? livestreamId)
        {
            var targetCount = new[] { orderId, productId, livestreamId }.Count(id => id.HasValue);

            if (targetCount != 1)
                throw new ArgumentException("Phải chỉ định đúng 1 loại review (Order, Product, hoặc Livestream)");
        }

        private static void ValidateRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating phải từ 1 đến 5 sao");
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   Rating >= 1 && Rating <= 5 &&
                   !string.IsNullOrWhiteSpace(ReviewText) &&
                   AccountID != Guid.Empty;
        }
    }
}