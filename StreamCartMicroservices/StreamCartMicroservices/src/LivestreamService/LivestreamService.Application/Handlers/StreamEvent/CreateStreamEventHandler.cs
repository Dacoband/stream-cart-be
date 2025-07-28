using LivestreamService.Application.Commands.StreamEvent;
using LivestreamService.Application.DTOs.StreamEvent;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.StreamEvent
{
    public class CreateStreamEventHandler : IRequestHandler<CreateStreamEventCommand, StreamEventDTO>
    {
        private readonly IStreamEventRepository _repository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ILogger<CreateStreamEventHandler> _logger;

        public CreateStreamEventHandler(
            IStreamEventRepository repository,
            ILivestreamRepository livestreamRepository,
            IAccountServiceClient accountServiceClient,
            ILogger<CreateStreamEventHandler> logger)
        {
            _repository = repository;
            _livestreamRepository = livestreamRepository;
            _accountServiceClient = accountServiceClient;
            _logger = logger;
        }

        public async Task<StreamEventDTO> Handle(CreateStreamEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify livestream exists
                var livestream = await _livestreamRepository.GetByIdAsync(request.LivestreamId.ToString());
                if (livestream == null)
                    throw new KeyNotFoundException($"Livestream {request.LivestreamId} not found");

                // Create stream event
                var streamEvent = new Domain.Entities.StreamEvent(
                    request.LivestreamId,
                    request.UserId,
                    request.EventType,
                    request.Payload,
                    request.LivestreamProductId,
                    request.UserId.ToString()
                );

                await _repository.InsertAsync(streamEvent);

                // Get user info for response
                var user = await _accountServiceClient.GetAccountByIdAsync(request.UserId);

                return new StreamEventDTO
                {
                    Id = streamEvent.Id,
                    LivestreamId = streamEvent.LivestreamId,
                    UserId = streamEvent.UserId,
                    LivestreamProductId = streamEvent.LivestreamProductId,
                    EventType = streamEvent.EventType,
                    Payload = streamEvent.Payload,
                    CreatedAt = streamEvent.CreatedAt,
                    CreatedBy = streamEvent.CreatedBy,
                    UserName = user?.Username
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stream event for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}