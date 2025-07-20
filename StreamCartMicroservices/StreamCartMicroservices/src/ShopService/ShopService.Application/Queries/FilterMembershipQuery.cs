using MediatR;
using Shared.Common.Models;
using ShopService.Application.DTOs.Membership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries
{
    public class FilterMembershipQuery : IRequest<ApiResponse<ListMembershipDTO>>
    {
        public required FilterMembershipDTO filter {  get; set; }
        
    }
}
