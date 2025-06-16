using MediatR;
using ProductService.Application.DTOs.Variants;
using System;
using System.Collections.Generic;

namespace ProductService.Application.Commands.VariantCommands
{
    public class CreateProductVariantCommand : IRequest<ProductVariantDto>
    {
        public Guid ProductId { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int Stock { get; set; }
        public string CreatedBy { get; set; }
    }

    public class UpdateProductVariantCommand : IRequest<ProductVariantDto>
    {
        public Guid Id { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public int Stock { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class DeleteProductVariantCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string DeletedBy { get; set; }
    }

    public class UpdateVariantStockCommand : IRequest<ProductVariantDto>
    {
        public Guid Id { get; set; }
        public int Stock { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class UpdateVariantPriceCommand : IRequest<ProductVariantDto>
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public decimal? FlashSalePrice { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class BulkUpdateVariantStockCommand : IRequest<bool>
    {
        public List<VariantStockUpdate> StockUpdates { get; set; } = new List<VariantStockUpdate>();
        public string UpdatedBy { get; set; }
    }

    public class CheckVariantStockCommand : IRequest<bool>
    {
        public Guid VariantId { get; set; }
        public int RequestedQuantity { get; set; }
    }
}