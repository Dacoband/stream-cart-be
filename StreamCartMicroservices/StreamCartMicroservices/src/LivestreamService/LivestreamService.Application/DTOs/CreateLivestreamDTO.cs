using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LivestreamService.Application.DTOs
{
    public class CreateLivestreamDTO
    {
        [Required]
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public DateTime ScheduledStartTime { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        // ✅ THÊM DANH SÁCH SẢN PHẨM
        public List<CreateLivestreamProductItemDTO>? Products { get; set; } = new();
    }

    public class CreateLivestreamProductItemDTO
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        public string? VariantId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; } // Null = dùng giá gốc

        [Range(0, int.MaxValue)]
        public int? Stock { get; set; } // Null = dùng tồn kho gốc

        public bool IsPin { get; set; } = false;

        //public Guid? FlashSaleId { get; set; }

        //[Range(0, int.MaxValue)]
        //public int DisplayOrder { get; set; } = 0;
    }
}