using ShopService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs.Membership
{
    public class UpdateMembershipDTO
    {
        public string? Name { get; set; }
        public MembershipType? Type { get; set; }
        public string? Description { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị tối thiểu là 0đ")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thời hạn ít nhất là 1 tháng")]
        public int? Duration { get; set; } // in days, weeks, etc.

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng sản phẩm tối đa phải lớn hơn 0")]
        public int? MaxProduct { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian Livestream tối đa phải lớn hơn 0(phút)")]
        public int? MaxLivestream { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm hoa hồng phải từ 0-100%")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Commission { get; set; }
    }
}
