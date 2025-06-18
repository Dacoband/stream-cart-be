using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs
{
    public class AddMemberDto
    {
        public Guid AccountId { get; set; }
        public string Role { get; set; } = "Staff";
    }
}
