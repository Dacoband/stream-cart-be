using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Products
{
    public class UpdateProductQuantitySoldDto
    {
        [Required]
        public int QuantityChange { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
