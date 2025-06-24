using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.FlashSale
{
    public class CreateFlashSaleDTO
    {
        public Guid ProductId { get; set; }
        public List<Guid>? VariantId { get; set; }
        [Range(100, double.MaxValue, ErrorMessage = "Giá FalshSale phải lớn hơn 100đ")]
        public decimal FLashSalePrice { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm áp dụng FlashSale phải lớn hơn 0")]
        public int? QuantityAvailable { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1) StartTime > now
            if (StartTime <= DateTime.UtcNow)
            {
                yield return new ValidationResult(
                    "Thời gian bắt đầu phải ở tương lai",
                    new[] { nameof(StartTime) });
            }

            // 2) EndTime > StartTime
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "Thời gian kết thúc phải sau thời gian bắt đầu",
                    new[] { nameof(EndTime) });
            }
        }
    }
}
