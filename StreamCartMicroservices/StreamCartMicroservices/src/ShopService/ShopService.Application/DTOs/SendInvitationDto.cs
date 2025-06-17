using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs
{
    public class SendInvitationDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Staff";
    }
}
