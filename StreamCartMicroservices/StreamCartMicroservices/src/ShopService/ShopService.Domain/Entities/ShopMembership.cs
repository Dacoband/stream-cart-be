using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Domain.Entities
{
    public class ShopMembership : BaseEntity
    {
        public Guid MembershipID { get; set; }
        public Guid ShopID { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Membership Membership { get; set; }
        public Shop Shop { get; set; }
    }
}
