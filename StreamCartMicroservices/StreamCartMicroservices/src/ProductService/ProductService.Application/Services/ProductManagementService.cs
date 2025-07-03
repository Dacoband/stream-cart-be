using MassTransit;
using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries.DetailQueries;
using ProductService.Application.Queries.ProductQueries;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class ProductManagementService : IProductService
    {
        private readonly IMediator _mediator;

        public ProductManagementService(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, string createdBy)
        {
            var command = new CreateProductCommand
            {
                ProductName = createProductDto.ProductName ?? string.Empty,
                Description = createProductDto.Description ?? string.Empty,
                SKU = createProductDto.SKU ?? string.Empty,
                CategoryId = createProductDto.CategoryId,
                BasePrice = createProductDto.BasePrice,
                DiscountPrice = createProductDto.DiscountPrice,
                StockQuantity = createProductDto.StockQuantity,
                Weight = createProductDto.Weight,
                Dimensions = createProductDto.Dimensions ?? string.Empty,
                HasVariant = createProductDto.HasVariant,
                ShopId = createProductDto.ShopId,
                CreatedBy = createdBy
            };
            return await _mediator.Send(command);
        }

        public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto, string updatedBy)
        {
            var command = new UpdateProductCommand
            {
                Id = id,
                ProductName = updateProductDto.ProductName,
                Description = updateProductDto.Description,
                SKU = updateProductDto.SKU,
                CategoryId = updateProductDto.CategoryId,
                BasePrice = updateProductDto.BasePrice,
                DiscountPrice = updateProductDto.DiscountPrice,
                Weight = updateProductDto.Weight,
                Dimensions = updateProductDto.Dimensions,
                HasVariant = updateProductDto.HasVariant,
                UpdatedBy = updatedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<bool> DeleteProductAsync(Guid id, string deletedBy)
        {
            var command = new DeleteProductCommand
            {
                Id = id,
                DeletedBy = deletedBy
            };
            return await _mediator.Send(command);
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            var query = new GetProductByIdQuery { Id = id };
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(bool activeOnly = false)
        {
            var query = new GetAllProductsQuery { ActiveOnly = activeOnly };
            return await _mediator.Send(query);
        }

        public async Task<PagedResult<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize, ProductSortOption sortOption, bool activeOnly, Guid? shopId, Guid? categoryId)
        {
            var query = new GetPagedProductsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortOption = sortOption,
                ActiveOnly = activeOnly,
                ShopId = shopId,
                CategoryId = categoryId
            };
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByShopIdAsync(Guid shopId, bool activeOnly = false)
        {
            var query = new GetProductsByShopIdQuery { ShopId = shopId, ActiveOnly = activeOnly };
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(Guid categoryId, bool activeOnly = false)
        {
            var query = new GetProductsByCategoryIdQuery { CategoryId = categoryId, ActiveOnly = activeOnly };
            return await _mediator.Send(query);
        }

        public async Task<IEnumerable<ProductDto>> GetBestSellingProductsAsync(int count, Guid? shopId, Guid? categoryId)
        {
            var query = new GetBestSellingProductsQuery { Count = count, ShopId = shopId, CategoryId = categoryId };
            return await _mediator.Send(query);
        }

        public async Task<ProductDto> UpdateProductStatusAsync(Guid id, bool isActive, string updatedBy)
        {
            var command = new UpdateProductStatusCommand { Id = id, IsActive = isActive, UpdatedBy = updatedBy };
            return await _mediator.Send(command);
        }

        public async Task<ProductDto> UpdateProductStockAsync(Guid id, int quantity, string updatedBy)
        {
            var command = new UpdateProductStockCommand { Id = id, StockQuantity = quantity, UpdatedBy = updatedBy };
            return await _mediator.Send(command);
        }

        public async Task<bool> CheckProductStockAsync(Guid id, int requestedQuantity)
        {
            var command = new CheckProductStockCommand { ProductId = id, RequestedQuantity = requestedQuantity };
            return await _mediator.Send(command);
        }

        public async Task<ProductDetailDto?> GetProductDetailAsync(Guid id)
        {
            var query = new GetProductDetailQuery { ProductId = id };
            return await _mediator.Send(query);
        }
        public async Task<ProductDto> CreateCompleteProductAsync(CompleteProductDto completeProductDto, string createdBy)
        {
            var command = new CreateCompleteProductCommand
            {
                CompleteProduct = completeProductDto,
                CreatedBy = createdBy
            };

            return await _mediator.Send(command);
        }
    }
}