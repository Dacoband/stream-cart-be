using LivestreamService.Application.Commands.LiveStreamService;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.Livestream
{
    public class UpdateMaxViewerHandler : IRequestHandler<UpdateMaxViewerCommand, LivestreamDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILogger<UpdateMaxViewerHandler> _logger;

        public UpdateMaxViewerHandler(
            ILivestreamRepository livestreamRepository,
            ILogger<UpdateMaxViewerHandler> logger)
        {
            _livestreamRepository = livestreamRepository;
            _logger = logger;
        }

        public async Task<LivestreamDTO> Handle(UpdateMaxViewerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var livestream = await _livestreamRepository.GetByIdAsync(request.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException($"Livestream {request.LivestreamId} not found");
                }

                // Verify seller owns the livestream
                if (livestream.SellerId != request.SellerId)
                {
                    throw new UnauthorizedAccessException("You can only update your own livestream");
                }

                // Update max viewer count
                livestream.SetMaxViewer(request.MaxViewer, request.SellerId.ToString());

                await _livestreamRepository.ReplaceAsync(livestream.Id.ToString(), livestream);

                _logger.LogInformation("Updated MaxViewer for livestream {LivestreamId} to {MaxViewer}",
                    request.LivestreamId, request.MaxViewer);

                return new LivestreamDTO
                {
                    Id = livestream.Id,
                    Title = livestream.Title,
                    Description = livestream.Description,
                    SellerId = livestream.SellerId,
                    ShopId = livestream.ShopId,
                    MaxViewer = livestream.MaxViewer,
                    Status = livestream.Status,
                    ScheduledStartTime = livestream.ScheduledStartTime,
                    ActualStartTime = livestream.ActualStartTime,
                    ActualEndTime = livestream.ActualEndTime,
                    ThumbnailUrl = livestream.ThumbnailUrl,
                    Tags = livestream.Tags,
                    //CreatedAt = livestream.CreatedAt,
                    //LastModifiedAt = livestream.LastModifiedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating max viewer count for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}