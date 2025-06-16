using MediatR;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class DeleteProductVariantCommandHandler : IRequestHandler<DeleteProductVariantCommand, bool>
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IProductCombinationRepository _combinationRepository;

        public DeleteProductVariantCommandHandler(
            IProductVariantRepository variantRepository,
            IProductCombinationRepository combinationRepository)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
            _combinationRepository = combinationRepository ?? throw new ArgumentNullException(nameof(combinationRepository));
        }

        public async Task<bool> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
        {
            var variant = await _variantRepository.GetByIdAsync(request.Id.ToString());
            if (variant == null)
            {
                return false;
            }

            // Delete all combinations related to this variant
            await _combinationRepository.DeleteByVariantIdAsync(request.Id);

            // Delete the variant
            await _variantRepository.DeleteAsync(variant.Id.ToString());

            return true;
        }
    }
}