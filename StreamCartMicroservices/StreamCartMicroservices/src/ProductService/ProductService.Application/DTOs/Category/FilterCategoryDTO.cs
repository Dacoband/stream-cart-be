using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Category
{
    public class FilterCategoryDTO
    {
        public string? CategoryName { get; set; } = string.Empty;
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; } = 10;
        public bool? IsDeleted { get; set; }
    }
}
