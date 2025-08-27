using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs.FlashSale
{
    public class CreateFlashSaleDTO
    {
        public List<CreateFlashSaleProductDTO> Products { get; set; } = new();

        [Range(1, 8, ErrorMessage = "Slot phải từ 1 đến 8")]
        public int Slot { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm áp dụng FlashSale phải lớn hơn 0")]
        public int? QuantityAvailable => Products?.Sum(p => p.QuantityAvailable ?? 0) > 0
            ? Products.Sum(p => p.QuantityAvailable ?? 0)
            : null;

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartTime <= DateTime.UtcNow)
            {
                yield return new ValidationResult(
                    "Thời gian bắt đầu phải ở tương lai",
                    new[] { nameof(StartTime) });
            }

            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "Thời gian kết thúc phải sau thời gian bắt đầu",
                    new[] { nameof(EndTime) });
            }

            if (Products == null || !Products.Any())
            {
                yield return new ValidationResult(
                    "Phải có ít nhất một sản phẩm để tạo FlashSale",
                    new[] { nameof(Products) });
            }

            for (int i = 0; i < Products.Count; i++)
            {
                var product = Products[i];
                if (product.FlashSalePrice <= 0)
                {
                    yield return new ValidationResult(
                        $"Sản phẩm thứ {i + 1} phải có giá FlashSale hợp lệ (> 0)",
                        new[] { $"Products[{i}].FlashSalePrice" });
                }
            }
        }

        public void ConvertToUtc()
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            var localStartTime = DateTime.SpecifyKind(StartTime, DateTimeKind.Unspecified);
            var localEndTime = DateTime.SpecifyKind(EndTime, DateTimeKind.Unspecified);

            StartTime = TimeZoneInfo.ConvertTimeToUtc(localStartTime, vnTimeZone);
            EndTime = TimeZoneInfo.ConvertTimeToUtc(localEndTime, vnTimeZone);
        }
    }

    public class CreateFlashSaleProductDTO
    {
        public Guid ProductId { get; set; }
        public List<Guid>? VariantIds { get; set; }
        [Range(100, double.MaxValue, ErrorMessage = "Giá FlashSale phải từ 100đ trở lên")]
        public decimal FlashSalePrice { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng FlashSale phải từ 1 trở lên")]
        public int? QuantityAvailable { get; set; }
    }
}