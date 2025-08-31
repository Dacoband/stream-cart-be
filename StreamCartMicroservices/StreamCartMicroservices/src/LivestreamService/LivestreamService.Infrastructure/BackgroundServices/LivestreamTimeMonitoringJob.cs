using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using LivestreamService.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Infrastructure.BackgroundServices
{
    public class LivestreamTimeMonitoringJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LivestreamTimeMonitoringJob> _logger;

        public LivestreamTimeMonitoringJob(
            IServiceProvider serviceProvider,
            ILogger<LivestreamTimeMonitoringJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting livestream time monitoring job at {Time}", DateTime.UtcNow);

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

                _logger.LogInformation("Found {Count} active livestreams to monitor", activeLivestreams.Count());

                foreach (var livestream in activeLivestreams)
                {
                    try
                    {
                        await ProcessLivestreamTimeMonitoring(livestream, membershipService, notificationService);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing time monitoring for livestream {LivestreamId}", livestream.Id);
                    }
                }

                _logger.LogInformation("Livestream time monitoring job completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in livestream time monitoring job");
            }
        }

        private async Task<IEnumerable<Livestream>> GetActiveLivestreamsAsync(ILivestreamRepository livestreamRepository)
        {
            var allLivestreams = await livestreamRepository.GetAllAsync();
            return allLivestreams.Where(l =>
                l.ActualStartTime.HasValue &&
                !l.ActualEndTime.HasValue &&
                !l.IsDeleted);
        }

        private async Task ProcessLivestreamTimeMonitoring(
            Livestream livestream,
            ILivestreamMembershipService membershipService,
            ILivestreamNotificationService notificationService)
        {
            if (!livestream.ActualStartTime.HasValue)
            {
                return;
            }

            var runningTime = DateTime.UtcNow - livestream.ActualStartTime.Value;
            var runningMinutes = (int)runningTime.TotalMinutes;

            var validation = await membershipService.ValidateRemainingLivestreamTimeAsync(livestream.ShopId);

            if (!validation.IsValid || validation.Membership == null)
            {
                _logger.LogWarning("Invalid membership for livestream {LivestreamId}, shop {ShopId}",
                    livestream.Id, livestream.ShopId);
                return;
            }

            var remainingMinutes = validation.RemainingMinutes;
            var usedMinutes = Math.Min(runningMinutes, remainingMinutes);

            if (remainingMinutes > 0 && remainingMinutes <= 20)
            {
                if (!await HasWarningBeenSentAsync(livestream.Id, remainingMinutes))
                {
                    await notificationService.SendLivestreamTimeWarningAsync(
                        livestream.Id,
                        livestream.SellerId,
                        remainingMinutes);

                    await MarkWarningAsSentAsync(livestream.Id, remainingMinutes);
                }
            }

            if (remainingMinutes <= 0 || runningMinutes >= validation.RemainingMinutes)
            {
                await notificationService.SendLivestreamTimeExpiredAsync(
                    livestream.Id,
                    livestream.SellerId);
                await AutoEndLivestreamAsync(livestream, membershipService);

                _logger.LogInformation("Auto-ended livestream {LivestreamId} due to membership time expiry", livestream.Id);
            }

            _logger.LogDebug("Monitored livestream {LivestreamId}: Running {RunningMinutes}min, Remaining {RemainingMinutes}min",
                livestream.Id, runningMinutes, remainingMinutes);
        }

        private async Task<bool> HasWarningBeenSentAsync(Guid livestreamId, int remainingMinutes)
        {

            return false;
        }

        private async Task MarkWarningAsSentAsync(Guid livestreamId, int remainingMinutes)
        {

            await Task.CompletedTask;
        }

        private async Task AutoEndLivestreamAsync(Livestream livestream, ILivestreamMembershipService membershipService)
        {
            try
            {
                var totalUsedMinutes = livestream.ActualStartTime.HasValue
                    ? (int)(DateTime.UtcNow - livestream.ActualStartTime.Value).TotalMinutes
                    : 0;

                livestream.End("system-membership-timeout");
                await membershipService.DeductLivestreamTimeAsync(livestream.ShopId, totalUsedMinutes);

                _logger.LogInformation("Auto-ended livestream {LivestreamId} and deducted {UsedMinutes} minutes from shop {ShopId}",
                    livestream.Id, totalUsedMinutes, livestream.ShopId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-ending livestream {LivestreamId}", livestream.Id);
            }
        }
    }
}