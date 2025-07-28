using MediatR;
using ProductService.Application.DTOs;
using System;

namespace ProductService.Application.Commands.ProductComands
{
    public class CreateProductCommand : IRequest<ProductDto>
    {
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public decimal? Weight { get; set; }
        //public string? Dimensions { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public bool HasVariant { get; set; }
        public Guid? ShopId { get; set; }
        public string? CreatedBy { get; set; }
    }
}