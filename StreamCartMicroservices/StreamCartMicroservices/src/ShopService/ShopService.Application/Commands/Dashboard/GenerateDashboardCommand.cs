using MediatR;
using ShopService.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Commands.Dashboard
{
    public class GenerateDashboardCommand : IRequest<ShopDashboardDTO>
    {
        public Guid ShopId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PeriodType { get; set; } = "daily"; 
        public string? GeneratedBy { get; set; } 
    }
}
