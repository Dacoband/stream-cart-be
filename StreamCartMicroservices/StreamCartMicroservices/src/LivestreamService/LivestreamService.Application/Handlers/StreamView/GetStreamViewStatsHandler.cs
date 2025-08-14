using LivestreamService.Application.DTOs.StreamView;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.StreamView;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.StreamView
{
    public class GetStreamViewStatsHandler : IRequestHandler<GetStreamViewStatsQuery, StreamViewStatsDTO>
    {
        private readonly IStreamViewRepository _repository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILogger<GetStreamViewStatsHandler> _logger;

        public GetStreamViewStatsHandler(
            IStreamViewRepository repository,
            ILivestreamRepository livestreamRepository,
            ILogger<GetStreamViewStatsHandler> logger)
        {
            _repository = repository;
            _livestreamRepository = livestreamRepository;
            _logger = logger;
        }

        public async Task<StreamViewStatsDTO> Handle(GetStreamViewStatsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var livestream = await _livestreamRepository.GetByIdAsync(request.LivestreamId.ToString());
                if (livestream == null)
                    throw new KeyNotFoundException($"Livestream {request.LivestreamId} not found");

                var totalViews = await _repository.CountTotalViewsAsync(request.LivestreamId);
                var currentViewers = await _repository.CountActiveViewersAsync(request.LivestreamId);
                var uniqueViewers = await _repository.CountUniqueViewersAsync(request.LivestreamId);
                var averageViewDuration = await _repository.GetAverageViewDurationAsync(request.LivestreamId);

                // ✅ GET ROLE-BASED VIEWER COUNTS
                var viewersByRole = await _repository.GetViewersByRoleAsync(request.LivestreamId);
                var currentCustomerViewers = viewersByRole.GetValueOrDefault("Customer", 0);

                // ✅ Get max customer viewer from livestream entity
                var maxCustomerViewer = livestream.MaxViewer ?? 0;

                // Calculate peak viewers (simplified - you might want to do this differently)
                var allViews = await _repository.GetByLivestreamIdAsync(request.LivestreamId);
                var peakViewers = allViews.Any() ? currentViewers : 0; // Simplified calculation

                return new StreamViewStatsDTO
                {
                    LivestreamId = request.LivestreamId,
                    TotalViews = totalViews,
                    CurrentViewers = currentViewers,
                    UniqueViewers = uniqueViewers,
                    AverageViewDuration = averageViewDuration,
                    PeakViewers = peakViewers,
                    PeakTime = DateTime.UtcNow, // Simplified

                    // ✅ CUSTOMER-SPECIFIC STATS
                    CurrentCustomerViewers = currentCustomerViewers,
                    MaxCustomerViewer = maxCustomerViewer, // This is the historical max customer count
                    ViewersByRole = viewersByRole,

                    // ✅ ADDITIONAL INSIGHTS
                    IsCurrentlyAtMaxRecord = currentCustomerViewers == maxCustomerViewer && maxCustomerViewer > 0,
                    MaxViewerAchievedAt = livestream.LastModifiedAt // Approximation
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream view stats for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}