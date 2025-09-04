using MassTransit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopService.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace ShopService.Application.DTOs.Membership
{
    public class CreateMembershipDTO
    {
        public string Name { get; set; }
        [Required]

        public MembershipType Type { get; set; }
        public string? Description { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị tối thiểu là 0đ")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int? Duration { get; set; } =0;

        public int? MaxModerator { get; set; } =0;

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian Livestream tối đa phải lớn hơn 0(phút)")]
        public int MaxLivestream { get; set; }

        public decimal? Commission { get; set; } = 0;
    }
}
