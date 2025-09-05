using MediatR;

namespace ProductService.Application.Commands.ProductCommands
{
    public class UpdateProductQuantitySoldCommand : IRequest<bool>
    {
        public Guid ProductId { get; set; }
        public int QuantityChange { get; set; }
        public string? UpdatedBy { get; set; }
    }
}