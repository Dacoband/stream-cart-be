using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class CreateReviewDTO
    {
        public Guid? OrderID { get; set; }
        public Guid? ProductID { get; set; }
        public Guid? LivestreamId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5 sao")]
        public int? Rating { get; set; }

        [StringLength(2000, ErrorMessage = "Nội dung review không được quá 2000 ký tự")]
        public string? ReviewText { get; set; } = string.Empty;

       // public List<string>? ImageUrls { get; set; }
    }
}
