using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Membership
{
    public class ListShopMembershipDTO
        
    {
        public int TotalItem {  get; set; }
        public List<DetailShopMembershipDTO> DetailShopMembership { get; set; }
    }
}
