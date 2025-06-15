using MediatR;
using ProductService.Application.DTOs;
using System;

namespace ProductService.Application.Commands
{
    public class CreateProductCommand : IRequest<ProductDto>
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public decimal? Weight { get; set; }
        public string Dimensions { get; set; }
        public bool HasVariant { get; set; }
        public Guid? ShopId { get; set; }
        public string CreatedBy { get; set; }
    }
    public class UpdateProductCommand : IRequest<ProductDto>
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string SKU { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? Weight { get; set; }
        public string Dimensions { get; set; }
        public bool? HasVariant { get; set; }
        public string UpdatedBy { get; set; }
    }
    public class DeleteProductCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string DeletedBy { get; set; }
    }
    public class UpdateProductStatusCommand : IRequest<ProductDto>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public string UpdatedBy { get; set; }
    }
    public class UpdateProductStockCommand : IRequest<ProductDto>
    {
        public Guid Id { get; set; }
        public int StockQuantity { get; set; }
        public string UpdatedBy { get; set; }
    }
    public class AssignProductToLivestreamCommand : IRequest<ProductDto>
    {
        public Guid ProductId { get; set; }
        public Guid? LivestreamId { get; set; }
        public string UpdatedBy { get; set; }
    }
    public class CheckProductStockCommand : IRequest<bool>
    {
        public Guid ProductId { get; set; }
        public int RequestedQuantity { get; set; }
    }
}