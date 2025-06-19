using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Combinations
{
    public class GenerateCombinationsDto
    {
        public List<AttributeValueGroup> AttributeValueGroups { get; set; } = new List<AttributeValueGroup>();
        public decimal DefaultPrice { get; set; }
        public int DefaultStock { get; set; }
    }

}
