using MediatR;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ImageHandlers
{
    public class ReorderProductImagesCommandHandler : IRequestHandler<ReorderProductImagesCommand, bool>
    {
        private readonly IProductImageRepository _imageRepository;

        public ReorderProductImagesCommandHandler(IProductImageRepository imageRepository)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<bool> Handle(ReorderProductImagesCommand request, CancellationToken cancellationToken)
        {
            var orderUpdates = request.ImagesOrder.Select(o => (o.ImageId, o.DisplayOrder)).ToList();

            // Update all images' display order
            var result = await _imageRepository.UpdateDisplayOrderAsync(orderUpdates);

            return result;
        }
    }
}