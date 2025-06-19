using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Combinations
{
    public class CreateProductCombinationDto
    {
        public Guid VariantId { get; set; }
        public Guid AttributeValueId { get; set; }
    }
}
