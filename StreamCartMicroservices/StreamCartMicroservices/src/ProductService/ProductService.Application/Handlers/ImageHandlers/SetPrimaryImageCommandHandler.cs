using MediatR;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ImageHandlers
{
    public class SetPrimaryImageCommandHandler : IRequestHandler<SetPrimaryImageCommand, bool>
    {
        private readonly IProductImageRepository _imageRepository;

        public SetPrimaryImageCommandHandler(IProductImageRepository imageRepository)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<bool> Handle(SetPrimaryImageCommand request, CancellationToken cancellationToken)
        {
            var image = await _imageRepository.GetByIdAsync(request.Id.ToString());
            if (image == null)
            {
                throw new ApplicationException($"Product image with ID {request.Id} not found");
            }

            if (request.IsPrimary)
            {
                // Reset other primary images in the same group first
                await _imageRepository.SetPrimaryImageAsync(image.Id, image.ProductId, image.VariantId);
            }

            // Update the image's primary status
            image.SetPrimary(request.IsPrimary);

            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                image.SetUpdatedBy(request.UpdatedBy);
            }

            await _imageRepository.ReplaceAsync(image.Id.ToString(), image);

            return true;
        }
    }
}