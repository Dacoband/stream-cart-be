using MediatR;
using ProductService.Application.Commands.AttributeValueCommands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.AttributeValueHandlers
{
    public class DeleteAttributeValueCommandHandler : IRequestHandler<DeleteAttributeValueCommand, bool>
    {
        private readonly IAttributeValueRepository _valueRepository;

        public DeleteAttributeValueCommandHandler(IAttributeValueRepository valueRepository)
        {
            _valueRepository = valueRepository ?? throw new ArgumentNullException(nameof(valueRepository));
        }

        public async Task<bool> Handle(DeleteAttributeValueCommand request, CancellationToken cancellationToken)
        {
            var attributeValue = await _valueRepository.GetByIdAsync(request.Id.ToString());
            if (attributeValue == null)
            {
                return false;
            }

            // Check if value is used in any combinations
            if (await _valueRepository.IsUsedInCombinationsAsync(request.Id))
            {
                throw new InvalidOperationException($"Cannot delete attribute value with ID {request.Id} because it is used in product combinations.");
            }

            await _valueRepository.DeleteAsync(attributeValue.Id.ToString());

            return true;
        }
    }
}