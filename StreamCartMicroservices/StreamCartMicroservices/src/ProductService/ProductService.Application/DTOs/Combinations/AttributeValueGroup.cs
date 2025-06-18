using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Combinations
{
    public class AttributeValueGroup
    {
        public Guid AttributeId { get; set; }
        public List<Guid> AttributeValueIds { get; set; } = new List<Guid>();
    }
}
