using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Common.Domain.Bases;
using System.Text.Json.Serialization;

namespace ProductService.Domain.Entities
{
    public class FlashSale : BaseEntity
    {
        //[Required]
        //[Column("LiveStreamProductID")]
        //public Guid LiveStreamProductId { get; set; }

        [Required]
        [Column("ProductID")]
        public Guid ProductId { get; set; }

        [Required]
        [Column("VariantID")]
        public Guid VariantId { get; set; }

        [Column("FlashSalePrice", TypeName = "decimal(10,2)")]
        public decimal FlashSalePrice { get; set; }

        [Column("QuantityAvailable")]
        public int QuantityAvailable { get; set; }

        [Column("QuantitySold")]
        public int QuantitySold { get; set; }

        [Column("StartTime")]
        public DateTime StartTime { get; set; }

        [Column("EndTime")]
        public DateTime EndTime { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        [JsonIgnore]

        public virtual Product Product { get; set; }

        [ForeignKey("VariantId")]
        public virtual ProductVariant ProductVariant { get; set; }
    }
}
