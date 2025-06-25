using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Category
{
    public class CategoryDetailDTO
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }

        public string? Description { get; set; }

        public string? IconURL { get; set; }

        public string? Slug { get; set; }
        public ICollection<CategoryDetailDTO>? SubCategories { get; set; }


    }
}
