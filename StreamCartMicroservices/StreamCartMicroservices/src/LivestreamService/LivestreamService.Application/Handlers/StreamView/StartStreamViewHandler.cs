using LivestreamService.Application.Commands.StreamView;
using LivestreamService.Application.DTOs.StreamView;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.StreamView
{
    public class StartStreamViewHandler : IRequestHandler<StartStreamViewCommand, StreamViewDTO>
    {
        private readonly IStreamViewRepository _repository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ILogger<StartStreamViewHandler> _logger;

        public StartStreamViewHandler(
            IStreamViewRepository repository,
            ILivestreamRepository livestreamRepository,
            IAccountServiceClient accountServiceClient,
            ILogger<StartStreamViewHandler> logger)
        {
            _repository = repository;
            _livestreamRepository = livestreamRepository;
            _accountServiceClient = accountServiceClient;
            _logger = logger;
        }

        public async Task<StreamViewDTO> Handle(StartStreamViewCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify livestream exists
                var livestream = await _livestreamRepository.GetByIdAsync(request.LivestreamId.ToString());
                if (livestream == null)
                    throw new KeyNotFoundException($"Livestream {request.LivestreamId} not found");

                // Check if user already has active view
                var existingView = await _repository.GetActiveViewByUserAsync(request.LivestreamId, request.UserId);
                if (existingView != null)
                {
                    // Return existing active view
                    var user = await _accountServiceClient.GetAccountByIdAsync(request.UserId);
                    return new StreamViewDTO
                    {
                        Id = existingView.Id,
                        LivestreamId = existingView.LivestreamId,
                        UserId = existingView.UserId,
                        StartTime = existingView.StartTime,
                        EndTime = existingView.EndTime,
                        IsActive = existingView.EndTime == null,
                        CreatedAt = existingView.CreatedAt,
                        UserName = user?.Username,
                        LivestreamTitle = livestream.Title
                    };
                }

                // Create new stream view
                var streamView = new Domain.Entities.StreamView(
                    request.LivestreamId,
                    request.UserId,
                    DateTime.UtcNow,
                    request.UserId.ToString()
                );

                await _repository.InsertAsync(streamView);

                // Get user info for response
                var userInfo = await _accountServiceClient.GetAccountByIdAsync(request.UserId);

                return new StreamViewDTO
                {
                    Id = streamView.Id,
                    LivestreamId = streamView.LivestreamId,
                    UserId = streamView.UserId,
                    StartTime = streamView.StartTime,
                    EndTime = streamView.EndTime,
                    IsActive = streamView.EndTime == null,
                    CreatedAt = streamView.CreatedAt,
                    UserName = userInfo?.Username,
                    LivestreamTitle = livestream.Title
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting stream view for user {UserId} in livestream {LivestreamId}",
                    request.UserId, request.LivestreamId);
                throw;
            }
        }
    }
}