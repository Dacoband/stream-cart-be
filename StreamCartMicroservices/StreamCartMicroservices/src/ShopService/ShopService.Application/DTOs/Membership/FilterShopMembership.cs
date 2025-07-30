using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Membership
{
    public class FilterShopMembership
    {
        public string ShopId { get; set; }
        public string? MembershipType {  get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
    }
}
