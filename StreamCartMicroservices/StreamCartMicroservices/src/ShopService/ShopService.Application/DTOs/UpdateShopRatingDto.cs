using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Application.DTOs
{
    public class UpdateShopRatingDto
    {
        [Required(ErrorMessage = "Rating không được để trống")]
        [Range(0, 5, ErrorMessage = "Rating phải từ 0 đến 5")]
        public decimal Rating { get; set; }

        public string? Modifier { get; set; }
    }
}
