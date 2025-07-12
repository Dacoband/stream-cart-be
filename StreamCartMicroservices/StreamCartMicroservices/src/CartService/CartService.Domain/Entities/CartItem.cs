using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        [Required]
        [ForeignKey("Cart")]
        public Guid CartId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        public Guid? VariantId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public Guid ShopId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ShopName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceSnapShot { get; set; } 

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceCurrent { get; set; }

        [Required]     
        public int Stock { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(500)]
        public string PrimaryImage { get; set; } = string.Empty;

        public bool ProductStatus { get; set; }


        [Column(TypeName = "jsonb")]
        public Dictionary<string, string>? Attributes { get; set; } = new();

        public Cart? Cart { get; set; }
    }
}
