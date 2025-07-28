using LivestreamService.Application.DTOs.StreamEvent;
using LivestreamService.Application.Interfaces;
using LivestreamService.Application.Queries.StreamEvent;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.StreamEvent
{
    public class GetStreamEventsByLivestreamHandler : IRequestHandler<GetStreamEventsByLivestreamQuery, IEnumerable<StreamEventDTO>>
    {
        private readonly IStreamEventRepository _repository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ILogger<GetStreamEventsByLivestreamHandler> _logger;

        public GetStreamEventsByLivestreamHandler(
            IStreamEventRepository repository,
            IAccountServiceClient accountServiceClient,
            ILogger<GetStreamEventsByLivestreamHandler> logger)
        {
            _repository = repository;
            _accountServiceClient = accountServiceClient;
            _logger = logger;
        }

        public async Task<IEnumerable<StreamEventDTO>> Handle(GetStreamEventsByLivestreamQuery request, CancellationToken cancellationToken)
        {
            try
            {
                IEnumerable<Domain.Entities.StreamEvent> events;

                if (!string.IsNullOrEmpty(request.EventType))
                {
                    events = await _repository.GetEventsByTypeAsync(request.LivestreamId, request.EventType);
                }
                else if (request.Count.HasValue)
                {
                    events = await _repository.GetRecentEventsByLivestreamAsync(request.LivestreamId, request.Count.Value);
                }
                else
                {
                    events = await _repository.GetByLivestreamIdAsync(request.LivestreamId);
                }

                var result = new List<StreamEventDTO>();
                foreach (var eventItem in events)
                {
                    var user = await _accountServiceClient.GetAccountByIdAsync(eventItem.UserId);
                    result.Add(new StreamEventDTO
                    {
                        Id = eventItem.Id,
                        LivestreamId = eventItem.LivestreamId,
                        UserId = eventItem.UserId,
                        LivestreamProductId = eventItem.LivestreamProductId,
                        EventType = eventItem.EventType,
                        Payload = eventItem.Payload,
                        CreatedAt = eventItem.CreatedAt,
                        CreatedBy = eventItem.CreatedBy,
                        UserName = user?.Username
                    });
                }

                return result.OrderByDescending(e => e.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stream events for livestream {LivestreamId}", request.LivestreamId);
                throw;
            }
        }
    }
}