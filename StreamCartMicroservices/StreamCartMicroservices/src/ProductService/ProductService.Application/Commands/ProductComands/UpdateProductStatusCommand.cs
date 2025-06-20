using MediatR;
using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Commands.ProductComands
{
    public class UpdateProductStatusCommand : IRequest<ProductDto>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
