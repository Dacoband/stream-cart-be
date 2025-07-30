using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Membership
{
    public class UpdateShopMembershipDTO
    {
        public string ShopId { get; set; }
        public int RemainingLivstream { get; set; }
    }
}
