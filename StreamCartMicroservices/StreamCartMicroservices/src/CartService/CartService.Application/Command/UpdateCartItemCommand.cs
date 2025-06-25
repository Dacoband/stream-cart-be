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
    public class UpdateCartItemCommand : IRequest<ApiResponse<UpdateCartItemDTO>>
    {
        public string UserId { get; set; }
        public Guid CartItemId { get; set; }
        public Guid? VariantId { get; set; }
        public int? Quantity { get; set; }
    }
}
