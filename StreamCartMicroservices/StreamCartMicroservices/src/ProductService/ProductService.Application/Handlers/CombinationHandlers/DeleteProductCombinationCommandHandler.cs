using MediatR;
using ProductService.Application.Commands.CombinationCommands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.CombinationHandlers
{
    public class DeleteProductCombinationCommandHandler : IRequestHandler<DeleteProductCombinationCommand, bool>
    {
        private readonly IProductCombinationRepository _combinationRepository;

        public DeleteProductCombinationCommandHandler(IProductCombinationRepository combinationRepository)
        {
            _combinationRepository = combinationRepository ?? throw new ArgumentNullException(nameof(combinationRepository));
        }

        public async Task<bool> Handle(DeleteProductCombinationCommand request, CancellationToken cancellationToken)
        {
            return await _combinationRepository.DeleteByVariantIdAndAttributeValueIdAsync(request.VariantId, request.AttributeValueId);
        }
    }
}