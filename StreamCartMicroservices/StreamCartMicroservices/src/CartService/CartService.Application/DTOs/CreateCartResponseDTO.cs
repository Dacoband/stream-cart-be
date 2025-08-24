using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.DTOs
{
    public class CreateCartResponseDTO
    {
        public string ProductId { get; set; }
        public string? VariantId { get; set; }

        public int Quantity { get; set; }
        public string CartItemId { get; set; }
    }
}
