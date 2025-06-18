using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Images
{
    public class ReorderImagesDto
    {
        public List<ImageOrderItem> ImagesOrder { get; set; } = new List<ImageOrderItem>();
    }
}
