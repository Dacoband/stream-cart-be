using MediatR;
using ProductService.Application.Commands.AttributeCommands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeHandlers
{
    public class DeleteProductAttributeCommandHandler : IRequestHandler<DeleteProductAttributeCommand, bool>
    {
        private readonly IProductAttributeRepository _attributeRepository;

        public DeleteProductAttributeCommandHandler(IProductAttributeRepository attributeRepository)
        {
            _attributeRepository = attributeRepository ?? throw new ArgumentNullException(nameof(attributeRepository));
        }

        public async Task<bool> Handle(DeleteProductAttributeCommand request, CancellationToken cancellationToken)
        {
            var attribute = await _attributeRepository.GetByIdAsync(request.Id.ToString());
            if (attribute == null)
            {
                return false;
            }

            // Check if attribute has associated values
            if (await _attributeRepository.HasAttributeValuesAsync(request.Id))
            {
                throw new InvalidOperationException($"Cannot delete attribute with ID {request.Id} because it has associated values.");
            }

            await _attributeRepository.DeleteAsync(attribute.Id.ToString());

            return true;
        }
    }
}