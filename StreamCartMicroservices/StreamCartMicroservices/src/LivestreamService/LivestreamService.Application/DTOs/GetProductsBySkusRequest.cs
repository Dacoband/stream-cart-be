using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Application.DTOs
{
    public class GetProductsBySkusRequest
    {
        [Required(ErrorMessage = "Danh sách SKU là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 SKU")]
        public List<string> Skus { get; set; } = new List<string>();
    }
}