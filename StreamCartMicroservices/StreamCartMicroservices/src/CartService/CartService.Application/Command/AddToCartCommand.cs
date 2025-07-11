using CartService.Application.DTOs;
using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Command
{
    public class AddToCartCommand : IRequest<ApiResponse<CreateCartDTO>>
    {
        public string UserId { get; set; }
        public string ProductId { get; set; }
        public string? VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
