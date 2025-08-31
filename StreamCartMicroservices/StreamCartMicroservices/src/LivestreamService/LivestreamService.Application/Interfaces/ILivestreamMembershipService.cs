using LivestreamService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.Interfaces
{
    public interface ILivestreamMembershipService
    {
        /// <summary>
        /// Validates if shop has remaining livestream time in their membership
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Validation result with remaining time</returns>
        Task<MembershipValidationResult> ValidateRemainingLivestreamTimeAsync(Guid shopId);

        /// <summary>
        /// Deducts used livestream time from shop's membership
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <param name="usedMinutes">Minutes used during livestream</param>
        /// <returns>True if successful</returns>
        Task<bool> DeductLivestreamTimeAsync(Guid shopId, int usedMinutes);

        /// <summary>
        /// Gets maximum allowed livestream duration for shop based on remaining time
        /// </summary>
        /// <param name="shopId">Shop ID</param>
        /// <returns>Max duration in minutes, null if no valid membership</returns>
        Task<int?> GetMaxLivestreamDurationAsync(Guid shopId);
    }
}
