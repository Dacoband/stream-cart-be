using System;
using System.ComponentModel.DataAnnotations;

namespace ShopService.Application.DTOs
{
    public class UpdateProductCountDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng sản phẩm không thể âm")]
        public int TotalProduct { get; set; }
    }
}
