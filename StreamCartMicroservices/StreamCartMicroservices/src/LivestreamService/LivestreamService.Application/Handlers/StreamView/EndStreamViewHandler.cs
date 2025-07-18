using LivestreamService.Application.Commands.StreamView;
using LivestreamService.Application.DTOs.StreamView;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.StreamView
{
    public class EndStreamViewHandler : IRequestHandler<EndStreamViewCommand, StreamViewDTO>
    {
        private readonly IStreamViewRepository _repository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ILogger<EndStreamViewHandler> _logger;

        public EndStreamViewHandler(
            IStreamViewRepository repository,
            ILivestreamRepository livestreamRepository,
            IAccountServiceClient accountServiceClient,
            ILogger<EndStreamViewHandler> logger)
        {
            _repository = repository;
            _livestreamRepository = livestreamRepository;
            _accountServiceClient = accountServiceClient;
            _logger = logger;
        }

        public async Task<StreamViewDTO> Handle(EndStreamViewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var streamView = await _repository.GetByIdAsync(request.StreamViewId.ToString());
                if (streamView == null)
                    throw new KeyNotFoundException($"Stream view {request.StreamViewId} not found");

                // Verify user owns this stream view
                if (streamView.UserId != request.UserId)
                    throw new UnauthorizedAccessException("You can only end your own stream view");

                // End the stream view
                streamView.EndView(DateTime.UtcNow, request.UserId.ToString());
                await _repository.ReplaceAsync(streamView.Id.ToString(), streamView);

                // Get user and livestream info for response
                var user = await _accountServiceClient.GetAccountByIdAsync(request.UserId);
                var livestream = await _livestreamRepository.GetByIdAsync(streamView.LivestreamId.ToString());

                return new StreamViewDTO
                {
                    Id = streamView.Id,
                    LivestreamId = streamView.LivestreamId,
                    UserId = streamView.UserId,
                    StartTime = streamView.StartTime,
                    EndTime = streamView.EndTime,
                    Duration = streamView.EndTime.HasValue ? streamView.EndTime.Value - streamView.StartTime : null,
                    IsActive = streamView.EndTime == null,
                    CreatedAt = streamView.CreatedAt,
                    UserName = user?.Username,
                    LivestreamTitle = livestream?.Title
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending stream view {StreamViewId}", request.StreamViewId);
                throw;
            }
        }
    }
}