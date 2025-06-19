using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Images
{
    public class CreateProductImageDto
    {
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string AltText { get; set; } = string.Empty;
    }
}
