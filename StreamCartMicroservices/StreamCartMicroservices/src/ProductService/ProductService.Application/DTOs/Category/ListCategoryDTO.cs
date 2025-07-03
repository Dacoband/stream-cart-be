using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Category
{
    public class ListCategoryDTO
    {
        public int TotalItem {  get; set; }
        public List<CategoryDetailDTO> Categories { get; set; }
    }
}
