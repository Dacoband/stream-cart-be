using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.FlashSale
{
    public class DeleteFlashSaleSlotDTO
    {
        /// <summary>
        /// Ngày của slot cần xóa
        /// </summary>
        [Required(ErrorMessage = "Ngày là bắt buộc")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Slot cần xóa (1-8)
        /// </summary>
        [Range(1, 8, ErrorMessage = "Slot phải từ 1 đến 8")]
        public int Slot { get; set; }

        /// <summary>
        /// Lý do xóa (tùy chọn)
        /// </summary>
        //[StringLength(500, ErrorMessage = "Lý do không được quá 500 ký tự")]
        //public string? Reason { get; set; }
    }
}
