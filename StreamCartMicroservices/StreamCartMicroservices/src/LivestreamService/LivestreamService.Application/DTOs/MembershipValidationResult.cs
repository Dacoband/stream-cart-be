using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivestreamService.Application.DTOs
{
    public class MembershipValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int RemainingMinutes { get; set; }
        public ShopMembershipDto? Membership { get; set; }
    }
}
