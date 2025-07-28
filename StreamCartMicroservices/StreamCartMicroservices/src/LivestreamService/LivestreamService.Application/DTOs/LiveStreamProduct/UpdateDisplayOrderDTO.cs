using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Application.DTOs
{
    public class UpdateDisplayOrderDTO
    {
        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải là số dương")]
        public int DisplayOrder { get; set; }
    }
}