using MediatR;
using ProductService.Application.Commands.ImageCommands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ImageHandlers
{
    public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, bool>
    {
        private readonly IProductImageRepository _imageRepository;

        public DeleteProductImageCommandHandler(IProductImageRepository imageRepository)
        {
            _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        }

        public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
        {
            var image = await _imageRepository.GetByIdAsync(request.Id.ToString());
            if (image == null)
            {
                return false;
            }

            // Use soft delete mechanism from BaseEntity
            image.Delete();

            if (!string.IsNullOrEmpty(request.DeletedBy))
            {
                image.SetUpdatedBy(request.DeletedBy);
            }

            await _imageRepository.ReplaceAsync(image.Id.ToString(), image);

            return true;
        }
    }
}