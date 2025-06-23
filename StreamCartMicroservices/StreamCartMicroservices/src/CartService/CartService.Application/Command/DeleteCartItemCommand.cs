using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Command
{
    public class DeleteCartItemCommand : IRequest<ApiResponse<bool>>
    {
        public Guid CartItemId { get; set; }
    }
}
