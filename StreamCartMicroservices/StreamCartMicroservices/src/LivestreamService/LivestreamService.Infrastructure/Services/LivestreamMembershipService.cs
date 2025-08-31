using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LivestreamService.Application.Services
{
    public class LivestreamMembershipService : ILivestreamMembershipService
    {
        private readonly IShopServiceClient _shopServiceClient;
        private readonly ILogger<LivestreamMembershipService> _logger;

        public LivestreamMembershipService(
            IShopServiceClient shopServiceClient,
            ILogger<LivestreamMembershipService> logger)
        {
            _shopServiceClient = shopServiceClient;
            _logger = logger;
        }

        public async Task<MembershipValidationResult> ValidateRemainingLivestreamTimeAsync(Guid shopId)
        {
            try
            {
                var membership = await _shopServiceClient.GetActiveShopMembershipAsync(shopId);

                if (membership == null)
                {
                    return new MembershipValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Shop không có gói thành viên hoạt động",
                        RemainingMinutes = 0
                    };
                }

                if (!membership.IsActive)
                {
                    return new MembershipValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Gói thành viên của shop đã hết hạn hoặc không hoạt động",
                        RemainingMinutes = 0
                    };
                }

                if (!membership.HasRemainingLivestreamTime)
                {
                    return new MembershipValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Shop đã hết thời gian livestream trong gói thành viên",
                        RemainingMinutes = 0
                    };
                }

                return new MembershipValidationResult
                {
                    IsValid = true,
                    RemainingMinutes = membership.RemainingLivestreamMinutes,
                    Membership = membership
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating membership for shop {ShopId}", shopId);
                return new MembershipValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Lỗi khi kiểm tra gói thành viên",
                    RemainingMinutes = 0
                };
            }
        }

        public async Task<bool> DeductLivestreamTimeAsync(Guid shopId, int usedMinutes)
        {
            try
            {
                var membership = await _shopServiceClient.GetActiveShopMembershipAsync(shopId);

                if (membership == null || !membership.IsActive)
                {
                    _logger.LogWarning("Cannot deduct livestream time: No active membership for shop {ShopId}", shopId);
                    return false;
                }

                var newRemainingMinutes = Math.Max(0, membership.RemainingLivestreamMinutes - usedMinutes);

                var success = await _shopServiceClient.UpdateShopMembershipRemainingLivestreamAsync(shopId, newRemainingMinutes);

                if (success)
                {
                    _logger.LogInformation("Deducted {UsedMinutes} minutes from shop {ShopId}. Remaining: {RemainingMinutes} minutes",
                        usedMinutes, shopId, newRemainingMinutes);
                }
                else
                {
                    _logger.LogWarning("Failed to deduct livestream time for shop {ShopId}", shopId);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting livestream time for shop {ShopId}", shopId);
                return false;
            }
        }

        public async Task<int?> GetMaxLivestreamDurationAsync(Guid shopId)
        {
            try
            {
                var membership = await _shopServiceClient.GetActiveShopMembershipAsync(shopId);

                if (membership == null || !membership.IsActive)
                {
                    return null;
                }

                return membership.RemainingLivestreamMinutes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting max livestream duration for shop {ShopId}", shopId);
                return null;
            }
        }
    }
}