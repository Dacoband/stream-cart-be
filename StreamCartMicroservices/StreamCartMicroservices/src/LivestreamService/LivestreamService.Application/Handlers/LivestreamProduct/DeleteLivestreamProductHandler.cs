using LivestreamService.Application.Commands.LiveStreamService;
using LivestreamService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LivestreamService.Application.Handlers.LivestreamProduct
{
    public class DeleteLivestreamProductHandler : IRequestHandler<DeleteLivestreamProductCommand, bool>
    {
        private readonly ILivestreamProductRepository _livestreamProductRepository;
        private readonly ILivestreamRepository _livestreamRepository;
        private readonly ILogger<DeleteLivestreamProductHandler> _logger;

        public DeleteLivestreamProductHandler(
            ILivestreamProductRepository livestreamProductRepository,
            ILivestreamRepository livestreamRepository,
            ILogger<DeleteLivestreamProductHandler> logger)
        {
            _livestreamProductRepository = livestreamProductRepository;
            _livestreamRepository = livestreamRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteLivestreamProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ SỬ DỤNG COMPOSITE KEY
                var livestreamProduct = await _livestreamProductRepository.GetByCompositeKeyAsync(
                    request.LivestreamId, request.ProductId, request.VariantId);

                if (livestreamProduct == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sản phẩm trong livestream {request.LivestreamId}, ProductId: {request.ProductId}, VariantId: {request.VariantId}");
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

                // Perform soft delete
                livestreamProduct.Delete();
                livestreamProduct.SetModifier(request.SellerId.ToString());

                // Save changes
                await _livestreamProductRepository.ReplaceAsync(livestreamProduct.Id.ToString(), livestreamProduct);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting livestream product for LivestreamId: {LivestreamId}, ProductId: {ProductId}, VariantId: {VariantId}",
                    request.LivestreamId, request.ProductId, request.VariantId);
                throw;
            }
        }
    }
}