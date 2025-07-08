using LivestreamService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.Queries
{
    public class GetShopLivestreamsQuery : IRequest<List<LivestreamDTO>>
    {
        public Guid ShopId { get; set; }
    }
}
