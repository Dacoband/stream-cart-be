using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.DTOs
{
    public class UpdateReviewDTO
    {
        [Required(ErrorMessage = "Nội dung review không được để trống")]
        [StringLength(2000, ErrorMessage = "Nội dung review không được quá 2000 ký tự")]
        public string ReviewText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rating không được để trống")]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        public List<string>? ImageUrls { get; set; }
    }
}