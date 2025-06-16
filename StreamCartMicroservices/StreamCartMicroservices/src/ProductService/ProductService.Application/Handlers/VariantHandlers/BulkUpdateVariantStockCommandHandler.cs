using MediatR;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class BulkUpdateVariantStockCommandHandler : IRequestHandler<BulkUpdateVariantStockCommand, bool>
    {
        private readonly IProductVariantRepository _variantRepository;

        public BulkUpdateVariantStockCommandHandler(IProductVariantRepository variantRepository)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
        }

        public async Task<bool> Handle(BulkUpdateVariantStockCommand request, CancellationToken cancellationToken)
        {
            if (request.StockUpdates == null || !request.StockUpdates.Any())
                return true; // Nothing to update

            var stockUpdates = new Dictionary<Guid, int>();

            foreach (var stockUpdate in request.StockUpdates)
            {
                if (stockUpdate.Quantity < 0)
                    throw new ArgumentException("Stock quantity cannot be negative");

                stockUpdates[stockUpdate.VariantId] = stockUpdate.Quantity;
            }

            return await _variantRepository.BulkUpdateStockAsync(stockUpdates);
        }
    }
}