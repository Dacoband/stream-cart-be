using MediatR;
using ShopService.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.Commands.Dashboard
{
    public class UpdateDashboardNotesCommand : IRequest<ShopDashboardDTO>
    {
        public Guid DashboardId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
