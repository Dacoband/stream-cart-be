using MassTransit;
using MediatR;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Variants;
using ProductService.Infrastructure.Interfaces;
using Shared.Messaging.Event.ProductEvent;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.VariantHandlers
{
    public class UpdateVariantPriceCommandHandler : IRequestHandler<UpdateVariantPriceCommand, ProductVariantDto>
    {
        private readonly IProductVariantRepository _variantRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateVariantPriceCommandHandler(IProductVariantRepository variantRepository, IPublishEndpoint publishEndpoint)
        {
            _variantRepository = variantRepository ?? throw new ArgumentNullException(nameof(variantRepository));
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ProductVariantDto> Handle(UpdateVariantPriceCommand request, CancellationToken cancellationToken)
        {
            var variant = await _variantRepository.GetByIdAsync(request.Id.ToString());
            if (variant == null)
            {
                throw new ApplicationException($"Product variant with ID {request.Id} not found");
            }

            variant.UpdatePrice(request.Price, request.FlashSalePrice);

            if (!string.IsNullOrEmpty(request.UpdatedBy))
            {
                variant.SetUpdatedBy(request.UpdatedBy);
            }

            await _variantRepository.ReplaceAsync(variant.Id.ToString(), variant);
            await _variantRepository.ReplaceAsync(variant.Id.ToString(), variant);
            try
            {
                var productEvent = new ProductUpdatedEvent()
                {
                    ProductId = variant.ProductId,
                    Price = (decimal)(variant.FlashSalePrice > 0 ? variant.FlashSalePrice : variant.Price),
                    Stock = variant.Stock,
                    ProductStatus = !variant.IsDeleted,
                    VariantId = variant.Id,
                };
                await _publishEndpoint.Publish(productEvent);
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return new ProductVariantDto
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                SKU = variant.SKU,
                Price = variant.Price,
                FlashSalePrice = variant.FlashSalePrice,
                Stock = variant.Stock,
                CreatedAt = variant.CreatedAt,
                CreatedBy = variant.CreatedBy,
                LastModifiedAt = variant.LastModifiedAt,
                LastModifiedBy = variant.LastModifiedBy ?? string.Empty
            };
        }
    }
}