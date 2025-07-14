using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Livestreamservice.Application.DTOs
{
    public class SellerDTO
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Fullname { get; set; }
        public string? AvatarUrl { get; set; }
        public Guid ShopId { get; set; }
       // public decimal CompleteRate { get; set; }
    }
}
