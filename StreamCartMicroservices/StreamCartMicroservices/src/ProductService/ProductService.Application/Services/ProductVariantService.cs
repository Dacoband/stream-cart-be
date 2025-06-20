using MediatR;
using ProductService.Application.Commands.VariantCommands;
using ProductService.Application.DTOs.Variants;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.VariantQueries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly IMediator _mediator;

        public ProductVariantService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IEnumerable<ProductVariantDto>> GetAllAsync()
        {
            return await _mediator.Send(new GetAllProductVariantsQuery());
        }

        public async Task<ProductVariantDto?> GetByIdAsync(Guid id)
        {
            return await _mediator.Send(new GetProductVariantByIdQuery { Id = id });
        }

        public async Task<ProductVariantDto> CreateAsync(CreateProductVariantDto dto, string createdBy)
        {
            var command = new CreateProductVariantCommand
            {
                ProductId = dto.ProductId,
                SKU = dto.SKU ?? string.Empty,
                Price = dto.Price,
                FlashSalePrice = dto.FlashSalePrice,
                Stock = dto.Stock,
                CreatedBy = createdBy
            };
            return await _mediator.Send(command);
        }

        public async Task<ProductVariantDto> UpdateAsync(Guid id, UpdateProductVariantDto dto, string updatedBy)
        {
            var command = new UpdateProductVariantCommand
            {
                Id = id,
                SKU = dto.SKU ?? string.Empty,
                Price = dto.Price,
                FlashSalePrice = dto.FlashSalePrice,
                Stock = dto.Stock,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteAsync(Guid id, string deletedBy)
        {
            var command = new DeleteProductVariantCommand
            {
                Id = id,
                DeletedBy = deletedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<IEnumerable<ProductVariantDto>> GetByProductIdAsync(Guid productId)
        {
            return await _mediator.Send(new GetVariantsByProductIdQuery { ProductId = productId });
        }

        public async Task<ProductVariantDto> UpdateStockAsync(Guid id, int quantity, string updatedBy)
        {
            var command = new UpdateVariantStockCommand
            {
                Id = id,
                Stock = quantity,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<ProductVariantDto> UpdatePriceAsync(Guid id, decimal price, decimal? flashSalePrice, string updatedBy)
        {
            var command = new UpdateVariantPriceCommand
            {
                Id = id,
                Price = price,
                FlashSalePrice = flashSalePrice,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<bool> BulkUpdateStockAsync(IEnumerable<BulkUpdateStockDto> stockUpdates, string updatedBy)
        {
            var command = new BulkUpdateVariantStockCommand
            {
                StockUpdates = (List<VariantStockUpdate>)stockUpdates,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }
    }
}