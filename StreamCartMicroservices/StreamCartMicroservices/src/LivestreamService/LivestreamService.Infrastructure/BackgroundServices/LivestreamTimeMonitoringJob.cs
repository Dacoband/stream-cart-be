using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.BackgroundServices
{
    [DisallowConcurrentExecution] // ✅ Đảm bảo không chạy đồng thời
    public class LivestreamTimeMonitoringJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LivestreamTimeMonitoringJob> _logger;

        // ✅ Cache để tránh gửi cảnh báo trùng lặp
        private static readonly ConcurrentDictionary<string, DateTime> _warningSentCache = new();

        public LivestreamTimeMonitoringJob(
            IServiceProvider serviceProvider,
            ILogger<LivestreamTimeMonitoringJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("🔍 Starting livestream time monitoring job at {Time}", DateTime.UtcNow);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var livestreamRepository = scope.ServiceProvider.GetRequiredService<ILivestreamRepository>();
                var membershipService = scope.ServiceProvider.GetRequiredService<ILivestreamMembershipService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<ILivestreamNotificationService>();

                // Get all active livestreams
                var activeLivestreams = await GetActiveLivestreamsAsync(livestreamRepository);

                if (!activeLivestreams.Any())
                {
                    _logger.LogInformation("No active livestreams found");
                    return;
                }

                _logger.LogInformation("📺 Found {Count} active livestreams to monitor", activeLivestreams.Count());

                foreach (var livestream in activeLivestreams)
                {
                    try
                    {
                        await ProcessLivestreamTimeMonitoring(
                            livestream,
                            membershipService,
                            notificationService,
                            livestreamRepository);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error processing time monitoring for livestream {LivestreamId}", livestream.Id);
                    }
                }

                _logger.LogInformation("✅ Livestream time monitoring job completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in livestream time monitoring job");
            }
        }

        private async Task<IEnumerable<Livestream>> GetActiveLivestreamsAsync(ILivestreamRepository livestreamRepository)
        {
            var allLivestreams = await livestreamRepository.GetAllAsync();
            return allLivestreams.Where(l =>
                l.ActualStartTime.HasValue &&
                !l.ActualEndTime.HasValue &&
                !l.IsDeleted &&
                l.Status); 
        }

        private async Task ProcessLivestreamTimeMonitoring(
            Livestream livestream,
            ILivestreamMembershipService membershipService,
            ILivestreamNotificationService notificationService,
            ILivestreamRepository livestreamRepository)
        {
            if (!livestream.ActualStartTime.HasValue)
            {
                _logger.LogWarning("⚠️ Livestream {LivestreamId} has no ActualStartTime", livestream.Id);
                return;
            }

            var runningTime = DateTime.UtcNow - livestream.ActualStartTime.Value;
            var runningMinutes = (int)runningTime.TotalMinutes;

            _logger.LogDebug("⏱️ Livestream {LivestreamId} has been running for {RunningMinutes} minutes",
                livestream.Id, runningMinutes);
            var validation = await membershipService.ValidateRemainingLivestreamTimeAsync(livestream.ShopId);

            if (!validation.IsValid || validation.Membership == null)
            {
                _logger.LogWarning("⚠️ Invalid membership for livestream {LivestreamId}, shop {ShopId}: {ErrorMessage}",
                    livestream.Id, livestream.ShopId, validation.ErrorMessage);

                await AutoEndLivestreamAsync(livestream, membershipService, livestreamRepository, "NO_VALID_MEMBERSHIP");
                return;
            }

            var totalRemainingMinutes = validation.RemainingMinutes;
            var actualRemainingMinutes = Math.Max(0, totalRemainingMinutes - runningMinutes);

            _logger.LogDebug("📊 Livestream {LivestreamId}: Total remaining {TotalRemaining}min, Used {UsedMinutes}min, Actual remaining {ActualRemaining}min",
                livestream.Id, totalRemainingMinutes, runningMinutes, actualRemainingMinutes);

            if (actualRemainingMinutes <= 0)
            {
                _logger.LogWarning("⏰ Livestream {LivestreamId} has exceeded time limit. Auto-ending...", livestream.Id);

                await notificationService.SendLivestreamTimeExpiredAsync(livestream.Id, livestream.SellerId);
                await AutoEndLivestreamAsync(livestream, membershipService, livestreamRepository, "TIME_EXPIRED");
                return;
            }
            await SendTimeWarningIfNeeded(livestream, actualRemainingMinutes, notificationService);

            _logger.LogDebug("✅ Processed livestream {LivestreamId}: {ActualRemaining} minutes remaining",
                livestream.Id, actualRemainingMinutes);
        }

        private async Task SendTimeWarningIfNeeded(
            Livestream livestream,
            int actualRemainingMinutes,
            ILivestreamNotificationService notificationService)
        {
            var warningThresholds = new[] { 20, 10, 5, 2, 1 };

            foreach (var threshold in warningThresholds)
            {
                if (actualRemainingMinutes == threshold)
                {
                    var cacheKey = $"{livestream.Id}_{threshold}";

                    if (_warningSentCache.TryGetValue(cacheKey, out var lastSent) &&
                        DateTime.UtcNow - lastSent < TimeSpan.FromMinutes(2))
                    {
                        return; 
                    }

                    try
                    {
                        await notificationService.SendLivestreamTimeWarningAsync(
                            livestream.Id,
                            livestream.SellerId,
                            actualRemainingMinutes);

                        // ✅ Đánh dấu đã gửi
                        _warningSentCache[cacheKey] = DateTime.UtcNow;

                        _logger.LogInformation("⚠️ Sent {Threshold}-minute warning for livestream {LivestreamId}",
                            threshold, livestream.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to send warning for livestream {LivestreamId}", livestream.Id);
                    }

                    break; 
                }
            }
        }

        private async Task<bool> HasWarningBeenSentAsync(Guid livestreamId, int remainingMinutes)
        {
            // ✅ Sử dụng cache thay vì database để tránh phức tạp
            var cacheKey = $"{livestreamId}_{remainingMinutes}";
            return _warningSentCache.ContainsKey(cacheKey) &&
                   DateTime.UtcNow - _warningSentCache[cacheKey] < TimeSpan.FromMinutes(2);
        }

        private async Task MarkWarningAsSentAsync(Guid livestreamId, int remainingMinutes)
        {
            // ✅ Lưu vào cache
            var cacheKey = $"{livestreamId}_{remainingMinutes}";
            _warningSentCache[cacheKey] = DateTime.UtcNow;

            // ✅ Clean up old cache entries (older than 1 hour)
            var expiredKeys = _warningSentCache
                .Where(kvp => DateTime.UtcNow - kvp.Value > TimeSpan.FromHours(1))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _warningSentCache.TryRemove(key, out _);
            }
        }

        private async Task AutoEndLivestreamAsync(
            Livestream livestream,
            ILivestreamMembershipService membershipService,
            ILivestreamRepository livestreamRepository,
            string reason)
        {
            try
            {
                _logger.LogInformation("🔄 Auto-ending livestream {LivestreamId} due to {Reason}", livestream.Id, reason);

                // ✅ Tính tổng thời gian đã sử dụng
                var totalUsedMinutes = livestream.ActualStartTime.HasValue
                    ? (int)(DateTime.UtcNow - livestream.ActualStartTime.Value).TotalMinutes
                    : 0;

                // ✅ Đảm bảo ít nhất 1 phút nếu đã start
                if (livestream.ActualStartTime.HasValue && totalUsedMinutes < 1)
                {
                    totalUsedMinutes = 1;
                }

                // ✅ Kết thúc livestream
                livestream.End($"system-{reason.ToLower()}");
                await livestreamRepository.ReplaceAsync(livestream.Id.ToString(), livestream);

                // ✅ Trừ thời gian membership
                if (totalUsedMinutes > 0)
                {
                    var deductionSuccess = await membershipService.DeductLivestreamTimeAsync(
                        livestream.ShopId, totalUsedMinutes);

                    if (deductionSuccess)
                    {
                        _logger.LogInformation("✅ Auto-ended livestream {LivestreamId} and deducted {UsedMinutes} minutes from shop {ShopId}",
                            livestream.Id, totalUsedMinutes, livestream.ShopId);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Auto-ended livestream {LivestreamId} but failed to deduct {UsedMinutes} minutes from shop {ShopId}",
                            livestream.Id, totalUsedMinutes, livestream.ShopId);
                    }
                }

                var keysToRemove = _warningSentCache.Keys
                    .Where(k => k.StartsWith(livestream.Id.ToString()))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _warningSentCache.TryRemove(key, out _);
                }

                _logger.LogInformation("🏁 Successfully auto-ended livestream {LivestreamId} due to {Reason}",
                    livestream.Id, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error auto-ending livestream {LivestreamId}", livestream.Id);
                throw; // Re-throw để có thể retry nếu cần
            }
        }
    }
}