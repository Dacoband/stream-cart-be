using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Images
{
    public class UpdateProductImageDto
    {
        public bool? IsPrimary { get; set; }
        public int? DisplayOrder { get; set; }
        public string? AltText { get; set; }
        public string? ImageUrl { get; set; }
    }
}
