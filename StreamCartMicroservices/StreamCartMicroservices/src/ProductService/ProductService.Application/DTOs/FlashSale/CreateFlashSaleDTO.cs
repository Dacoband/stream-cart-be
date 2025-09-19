using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        [Required(ErrorMessage = "Ngày áp dụng FlashSale là bắt buộc")]
        public DateTime Date { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date.Date <= DateTime.Now.Date)
            {
                yield return new ValidationResult(
                    "Ngày áp dụng FlashSale phải là ngày mai trở đi",
                    new[] { nameof(Date) });
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
    }

    public class VariantFlashSaleItemDto
    {
        public Guid VariantId { get; set; }
        [Range(100, double.MaxValue)]
        public decimal FlashSalePrice { get; set; }
        [Range(1, int.MaxValue)]
        public int? QuantityAvailable { get; set; }
    }

    public class CreateFlashSaleProductDTO
    {
        public Guid ProductId { get; set; }

        // OLD (giữ tương thích)
        public List<Guid?>? VariantIds { get; set; }
        public List<VariantFlashSaleItemDto>? VariantItems { get; set; }

        [Range(100, double.MaxValue, ErrorMessage = "Giá FlashSale phải từ 100đ trở lên")]
        public decimal FlashSalePrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng FlashSale phải từ 1 trở lên")]
        public int? QuantityAvailable { get; set; }
    }

    public class NullableGuidListConverter : JsonConverter<List<Guid?>?>
    {
        public override List<Guid?>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array");

            var list = new List<Guid?>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.Null)
                {
                    list.Add(null);
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    var guidString = reader.GetString();
                    if (Guid.TryParse(guidString, out var guid))
                    {
                        list.Add(guid);
                    }
                    else
                    {
                        list.Add(null);
                    }
                }
            }

            return list;
        }

        public override void Write(Utf8JsonWriter writer, List<Guid?>? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (var item in value)
            {
                if (item.HasValue)
                    writer.WriteStringValue(item.Value.ToString());
                else
                    writer.WriteNullValue();
            }
            writer.WriteEndArray();
        }
    }
}