using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Images
{
    public class ImageOrderItem
    {
        public Guid ImageId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
