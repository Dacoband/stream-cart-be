using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.Category
{
    public class CreateCatgoryDTO
    {
        public string CategoryName { get; set; }    
        public string? Description { get; set; }
        public string? IconURL { get; set; }
        public string? Slug { get; set; }
        public Guid? ParentCategoryID { get; set; } = Guid.Empty;
        
    }
}
