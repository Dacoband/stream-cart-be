using MediatR;
using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.ProductComands
{
    public class UpdateProductCommand : IRequest<ProductDto>
    {
        public Guid Id { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? SKU { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? Weight { get; set; }
        //public string? Dimensions { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public bool? HasVariant { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
