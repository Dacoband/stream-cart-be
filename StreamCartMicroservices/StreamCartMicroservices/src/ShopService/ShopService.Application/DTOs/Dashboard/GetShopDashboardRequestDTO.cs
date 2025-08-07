using System;

namespace ShopService.Application.DTOs.Dashboard
{
    public class GetShopDashboardRequestDTO
    {
        public Guid ShopId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PeriodType { get; set; } 
    }
    
    public class UpdateDashboardNotesDTO
    {
        public Guid ShopId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}