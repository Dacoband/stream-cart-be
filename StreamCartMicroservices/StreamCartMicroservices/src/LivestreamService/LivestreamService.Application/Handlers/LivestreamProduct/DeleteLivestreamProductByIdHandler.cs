using LivestreamService.Application.Commands.LiveStreamService;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.LivestreamProduct
{
    public class DeleteLivestreamProductByIdHandler : IRequestHandler<DeleteLivestreamProductByIdCommand, bool>
    {
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILogger<DeleteLivestreamProductByIdHandler> _logger;

        public DeleteLivestreamProductByIdHandler(
            ILivestreamProductRepository livestreamProductRepository,
            ILivestreamRepository livestreamRepository,
            ILogger<DeleteLivestreamProductByIdHandler> logger)
        {
            _livestreamProductRepository = livestreamProductRepository;
            _livestreamRepository = livestreamRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteLivestreamProductByIdCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var livestreamProduct = await _livestreamProductRepository.GetByIdAsync(request.Id.ToString());
                if (livestreamProduct == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID {request.Id}");
                }

                // Verify seller owns the livestream
                var livestream = await _livestreamRepository.GetByIdAsync(livestreamProduct.LivestreamId.ToString());
                if (livestream == null)
                {
                    throw new KeyNotFoundException("Livestream not found");
                }

                if (livestream.SellerId != request.SellerId)
                {
                    throw new UnauthorizedAccessException("You can only delete products from your own livestream");
                }

                await _livestreamProductRepository.DeleteAsync(request.Id.ToString());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting livestream product by ID {Id}", request.Id);
                throw;
            }
        }
    }
}
