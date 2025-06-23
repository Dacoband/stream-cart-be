using CartService.Application.DTOs;
using MediatR;
using Shared.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Query
{
    public class GetMyCartQuery : IRequest<ApiResponse<CartResponeDTO>>
    {
        public string userId;
    }
}
