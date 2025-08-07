using MediatR;
using ShopService.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Queries.Dashboard
{
    public class GetShopDashboardSummaryQuery : IRequest<ShopDashboardSummaryDTO>
    {
        public Guid ShopId { get; set; }
    }
}
