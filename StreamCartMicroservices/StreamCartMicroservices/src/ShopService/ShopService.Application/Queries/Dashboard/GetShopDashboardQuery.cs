using MediatR;
using ShopService.Application.DTOs.Dashboard;
using System;

namespace ShopService.Application.Queries.Dashboard
{
    public class GetShopDashboardQuery : IRequest<ShopDashboardDTO>
    {
        public Guid ShopId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string PeriodType { get; set; } = "daily"; 
    }
}