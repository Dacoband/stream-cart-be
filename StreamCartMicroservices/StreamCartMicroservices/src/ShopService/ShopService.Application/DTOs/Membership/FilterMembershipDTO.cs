using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Membership
{
    public class FilterMembershipDTO
    {
        public string? Type { get; set; }
        public decimal? FromPrice { get; set; }
        public decimal? ToPrice { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxProduct { get; set; }
        public int? MaxLivestream { get; set; }
        public int? MaxCommission { get; set; }

       // public bool? IsDeleted { get; set; } 

        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }

        public SortByMembershipEnum SortBy { get; set; } = SortByMembershipEnum.Name;
        public SortDirectionEnum SortDirection { get; set; } = SortDirectionEnum.Asc;
    }
}
