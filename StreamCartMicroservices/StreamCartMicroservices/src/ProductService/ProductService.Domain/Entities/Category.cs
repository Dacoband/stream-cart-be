using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Common.Domain.Bases;

namespace ProductService.Domain.Entities
{
    public class Category : BaseEntity
    {
        
        [Required]
        [MaxLength(255)]
        public string CategoryName { get; set; }

        public string? Description { get; set; }

        public string? IconURL { get; set; }

        public string? Slug { get; set; }

        public Guid? ParentCategoryID { get; set; }

        [ForeignKey("ParentCategoryID")]
        public Category? ParentCategory { get; set; }

        public ICollection<Category>? SubCategories { get; set; }

        public Category()
        {
            
        }

        public Category( string categoryName, string? description, string? iconURL, string? slug,  Guid? parentCategoryID)
        {
            CategoryName = categoryName;
            Description = description;
            IconURL = iconURL;
            Slug = slug;
            ParentCategoryID = parentCategoryID;
        }


    }
}
