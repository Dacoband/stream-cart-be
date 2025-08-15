using Livestreamservice.Application.Commands;
using LivestreamService.Application.Commands;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Common.Services.User;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class UpdateLivestreamCommandHandler : IRequestHandler<UpdateLivestreamCommand, LivestreamDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILogger<UpdateLivestreamCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateLivestreamCommandHandler(
            ILivestreamRepository livestreamRepository,
            ILogger<UpdateLivestreamCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _livestreamRepository = livestreamRepository;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<LivestreamDTO> Handle(UpdateLivestreamCommand request, CancellationToken cancellationToken)
        {
            var livestream = await _livestreamRepository.GetByIdAsync(request.Id.ToString());

            if (livestream == null)
            {
                throw new KeyNotFoundException($"Livestream with ID {request.Id} not found");
            }

            // ✅ FIX: Get requestingUserId from current user service
            var requestingUserId = _currentUserService.GetUserId();

            // Update the livestream properties with requestingUserId
            livestream.UpdateDetails(
                request.Title,
                request.Description,
                request.ScheduledStartTime,
                request.ThumbnailUrl,
                request.Tags,
                request.UpdatedBy,
                requestingUserId // ✅ ADD: Required parameter
            );

            // Save changes
            await _livestreamRepository.ReplaceAsync(livestream.Id.ToString(), livestream);

            _logger.LogInformation("Livestream {LivestreamId} updated by {UpdatedBy} (requesting user: {RequestingUserId})",
                livestream.Id, request.UpdatedBy, requestingUserId);

            return new LivestreamDTO
            {
                Id = livestream.Id,
                Title = livestream.Title,
                Description = livestream.Description,
                SellerId = livestream.SellerId,
                ShopId = livestream.ShopId,
                ScheduledStartTime = livestream.ScheduledStartTime,
                ActualStartTime = livestream.ActualStartTime,
                ActualEndTime = livestream.ActualEndTime,
                Status = livestream.Status,
                ThumbnailUrl = livestream.ThumbnailUrl,
                Tags = livestream.Tags,
                IsPromoted = livestream.IsPromoted
            };
        }
    }
}