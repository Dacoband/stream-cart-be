using MediatR;
using ProductService.Application.DTOs;
using ProductService.Application.DTOs.Products;

namespace ProductService.Application.Commands.ProductComands
{
    public class CreateCompleteProductCommand : IRequest<ProductDto>
    {
        public CompleteProductDto CompleteProduct { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
    }
}