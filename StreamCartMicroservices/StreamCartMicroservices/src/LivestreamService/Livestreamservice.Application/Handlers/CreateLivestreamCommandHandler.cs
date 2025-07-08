using LivestreamService.Application.Commands;
using LivestreamService.Application.DTOs;
using LivestreamService.Application.Interfaces;
using LivestreamService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers
{
    public class CreateLivestreamCommandHandler : IRequestHandler<CreateLivestreamCommand, LivestreamDTO>
    {
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILivekitService _livekitService;
        private readonly IShopServiceClient _shopServiceClient;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly ILogger<CreateLivestreamCommandHandler> _logger;

        public CreateLivestreamCommandHandler(
            ILivestreamRepository livestreamRepository,
            ILivekitService livekitService,
            IShopServiceClient shopServiceClient,
            IAccountServiceClient accountServiceClient,
            ILogger<CreateLivestreamCommandHandler> logger)
        {
            _livestreamRepository = livestreamRepository ?? throw new ArgumentNullException(nameof(livestreamRepository));
            _livekitService = livekitService ?? throw new ArgumentNullException(nameof(livekitService));
            _shopServiceClient = shopServiceClient ?? throw new ArgumentNullException(nameof(shopServiceClient));
            _accountServiceClient = accountServiceClient ?? throw new ArgumentNullException(nameof(accountServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LivestreamDTO> Handle(CreateLivestreamCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate shop
                var shop = await _shopServiceClient.GetShopByIdAsync(request.ShopId);
                if (shop == null)
                {
                    throw new ApplicationException($"Shop with ID {request.ShopId} not found");
                }

                // Verify seller account
                var seller = await _accountServiceClient.GetSellerByIdAsync(request.SellerId);
                if (seller == null)
                {
                    throw new ApplicationException($"Seller account with ID {request.SellerId} not found");
                }

                // Create LiveKit room
                string roomId = $"shop-{request.ShopId}-{Guid.NewGuid()}";
                string livekitRoomId = await _livekitService.CreateRoomAsync(roomId);

                // Generate unique stream key
                string streamKey = Guid.NewGuid().ToString("N");

                // Create livestream entity
                var livestream = new Livestream(
                    request.Title,
                    request.Description,
                    request.SellerId,
                    request.ShopId,
                    request.ScheduledStartTime,
                    livekitRoomId,
                    streamKey,
                    request.ThumbnailUrl,
                    request.Tags,
                    request.SellerId.ToString()
                );

                // Save livestream
                await _livestreamRepository.InsertAsync(livestream);

                // Generate join token for the seller (with publisher permissions)
                string joinToken = await _livekitService.GenerateJoinTokenAsync(
                    livekitRoomId,
                    request.SellerId.ToString(),
                    true // Can publish
                );

                // Return DTO
                return new LivestreamDTO
                {
                    Id = livestream.Id,
                    Title = livestream.Title,
                    Description = livestream.Description,
                    SellerId = livestream.SellerId,
                    SellerName = seller.Fullname ?? seller.Username,
                    ShopId = livestream.ShopId,
                    ShopName = shop.ShopName,
                    ScheduledStartTime = livestream.ScheduledStartTime,
                    ActualStartTime = livestream.ActualStartTime,
                    ActualEndTime = livestream.ActualEndTime,
                    Status = livestream.Status,
                    StreamKey = livestream.StreamKey,
                    LivekitRoomId = livekitRoomId,
                    JoinToken = joinToken,
                    ThumbnailUrl = livestream.ThumbnailUrl,
                    Tags = livestream.Tags,
                    IsPromoted = livestream.IsPromoted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating livestream: {Message}", ex.Message);
                throw;
            }
        }
    }
}