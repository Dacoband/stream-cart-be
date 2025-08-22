using MediatR;
using ProductService.Application.DTOs.Variants;

namespace ProductService.Application.Queries.VariantQueries
{
    public class GetProductVariantById1Query : IRequest<ProductVariantDto1>
    {
        public Guid Id { get; set; }
    }
}