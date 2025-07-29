using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Membership
{
    public class ListMembershipDTO
    {
        public List<DetailMembershipDTO> Memberships { get; set; }
        public int TotalItems { get; set; }
    }
}
