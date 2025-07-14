using Livestreamservice.Application.Commands;
using LivestreamService.Application.Commands;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class UpdateLivestreamCommandHandler : IRequestHandler<UpdateLivestreamCommand, LivestreamDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILogger<UpdateLivestreamCommandHandler> _logger;

        public UpdateLivestreamCommandHandler(
            ILivestreamRepository livestreamRepository,
            ILogger<UpdateLivestreamCommandHandler> logger)
        {
            _livestreamRepository = livestreamRepository;
            _logger = logger;
        }

        public async Task<LivestreamDTO> Handle(UpdateLivestreamCommand request, CancellationToken cancellationToken)
        {
            var livestream = await _livestreamRepository.GetByIdAsync(request.Id.ToString());

            if (livestream == null)
            {
                throw new KeyNotFoundException($"Livestream with ID {request.Id} not found");
            }

            // Update the livestream properties
            livestream.UpdateDetails(
                request.Title,
                request.Description,
                request.ScheduledStartTime,
                request.ThumbnailUrl,
                request.Tags,
                request.UpdatedBy
            );

            // Save changes
            await _livestreamRepository.ReplaceAsync(livestream.Id.ToString(), livestream);

            _logger.LogInformation("Livestream {LivestreamId} updated by {UpdatedBy}", livestream.Id, request.UpdatedBy);

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