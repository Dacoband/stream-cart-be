using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.DTOs.FlashSale
{
    public class UpdateFlashSalePriceQuantityDTO
    {
        /// <summary>
        /// Giá FlashSale mới
        /// </summary>
        [Range(100, double.MaxValue, ErrorMessage = "Giá FlashSale phải lớn hơn 100đ")]
        public decimal? FLashSalePrice { get; set; }

        /// <summary>
        /// Số lượng khả dụng mới
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm áp dụng FlashSale phải lớn hơn 0")]
        public int? QuantityAvailable { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ít nhất một trong hai field phải có giá trị
            if (!FLashSalePrice.HasValue && !QuantityAvailable.HasValue)
            {
                yield return new ValidationResult(
                    "Phải cung cấp ít nhất một trong hai: giá FlashSale hoặc số lượng",
                    new[] { nameof(FLashSalePrice), nameof(QuantityAvailable) });
            }
        }
    }
}
