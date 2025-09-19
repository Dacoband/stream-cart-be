using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProductService.Application.DTOs.FlashSale
{
    public class CreateFlashSaleDTO : IValidatableObject
    {
        [Required]
        public List<CreateFlashSaleProductDTO> Products { get; set; } = new();

        [Range(1, 8, ErrorMessage = "Slot phải từ 1 đến 8")]
        public int Slot { get; set; }

        [Required(ErrorMessage = "Ngày áp dụng FlashSale là bắt buộc")]
        public DateTime Date { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //if (Date.Date <= DateTime.UtcNow.Date)
            //{
            //    yield return new ValidationResult(
            //        "Ngày áp dụng FlashSale phải lớn hơn ngày hiện tại",
            //        new[] { nameof(Date) });
            //}

            if (Products == null || !Products.Any())
            {
                yield return new ValidationResult(
                    "Phải có ít nhất một sản phẩm",
                    new[] { nameof(Products) });
            }

            foreach (var (p, idx) in Products.Select((p, i) => (p, i)))
            {
                if ((p.VariantMap == null || p.VariantMap.Count == 0) && p.FlashSalePrice <= 0)
                {
                    yield return new ValidationResult(
                        $"Products[{idx}].FlashSalePrice phải > 0 (khi không dùng variant map)",
                        new[] { $"Products[{idx}].FlashSalePrice" });
                }

                if (p.VariantMap != null && p.VariantMap.Count > 0)
                {
                    foreach (var kv in p.VariantMap)
                    {
                        if (kv.Value.Price <= 0)
                        {
                            yield return new ValidationResult(
                                $"Variant {kv.Key} có giá không hợp lệ",
                                new[] { $"Products[{idx}].VariantMap[{kv.Key}]" });
                        }
                        if (kv.Value.Quantity.HasValue && kv.Value.Quantity <= 0)
                        {
                            yield return new ValidationResult(
                                $"Variant {kv.Key} quantity phải > 0",
                                new[] { $"Products[{idx}].VariantMap[{kv.Key}].quantity" });
                        }
                    }
                }
            }
        }
    }

    public class CreateFlashSaleProductDTO
    {
        [Required]
        public Guid ProductId { get; set; }

        /// <summary>
        /// Map variantId -> { price, quantity } hoặc chỉ số (price).
        /// </summary>
        [JsonPropertyName("variantMap")]
        public Dictionary<Guid, VariantFlashSaleValue>? VariantMap { get; set; }

        /// <summary>
        /// Giá FlashSale áp dụng cho toàn product (khi không có VariantMap)
        /// </summary>
        public decimal FlashSalePrice { get; set; }

        /// <summary>
        /// Số lượng áp dụng chung (fallback cho các variant không khai quantity)
        /// </summary>
        public int? QuantityAvailable { get; set; }
    }

    [JsonConverter(typeof(VariantFlashSaleValueConverter))]
    public class VariantFlashSaleValue
    {
        public decimal Price { get; set; }
        public int? Quantity { get; set; }
    }

    public class VariantFlashSaleValueConverter : JsonConverter<VariantFlashSaleValue>
    {
        public override VariantFlashSaleValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Nếu chỉ là số: coi như price
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (!reader.TryGetDecimal(out var price))
                    throw new JsonException("Invalid price number");
                return new VariantFlashSaleValue { Price = price };
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                decimal price = 0;
                int? quantity = null;

                if (root.TryGetProperty("price", out var priceProp))
                {
                    price = priceProp.GetDecimal();
                }
                else
                {
                    throw new JsonException("Missing 'price' in variantMap object value");
                }

                if (root.TryGetProperty("quantity", out var qtyProp) && qtyProp.ValueKind != JsonValueKind.Null)
                {
                    quantity = qtyProp.GetInt32();
                }

                return new VariantFlashSaleValue
                {
                    Price = price,
                    Quantity = quantity
                };
            }

            if (reader.TokenType == JsonTokenType.Null) return null;

            throw new JsonException("Invalid variant value format");
        }

        public override void Write(Utf8JsonWriter writer, VariantFlashSaleValue value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("price", value.Price);
            if (value.Quantity.HasValue)
            {
                writer.WriteNumber("quantity", value.Quantity.Value);
            }
            writer.WriteEndObject();
        }
    }
}