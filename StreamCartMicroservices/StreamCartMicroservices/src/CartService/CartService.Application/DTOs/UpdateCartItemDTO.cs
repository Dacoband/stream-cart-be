using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class UpdateCartItemDTO
    {
        public Guid CartItem { get; set; }
        public Guid? VariantId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm phải lớn hơn 0")]

        public int? Quantity { get; set; }
    }
}
