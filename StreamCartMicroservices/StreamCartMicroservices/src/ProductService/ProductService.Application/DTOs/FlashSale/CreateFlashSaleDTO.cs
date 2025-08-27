using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs.FlashSale
{
    public class CreateFlashSaleDTO
    {
        public List<CreateFlashSaleProductDTO> Products { get; set; } = new();

        [Range(1, 8, ErrorMessage = "Slot phải từ 1 đến 8")]
        public int Slot { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm FlashSale cho sản phẩm phải nằm trong khoảng 0%-100%")]
        public decimal FlashSalePrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm áp dụng FlashSale phải lớn hơn 0")]
        public int? QuantityAvailable { get; set; }

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
    }
}