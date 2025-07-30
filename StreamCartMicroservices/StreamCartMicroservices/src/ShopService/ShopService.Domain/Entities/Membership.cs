using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Domain.Entities
{
    public class Membership : BaseEntity
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? Duration { get; set; }
        public int? MaxProduct { get; set; }
        public int MaxLivestream { get; set; }
        public decimal? Commission { get; set; }
        public ICollection<ShopMembership> ShopMemberships { get; set; }

    }
}
