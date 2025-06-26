using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.DeliveryOrder
{
    public class GHNCalculateFeeRequest
    {
        [JsonPropertyName("to_district_id")]
        public int ToDistrictId { get; set; }
        [JsonPropertyName("to_ward_code")]
        public string ToWardCode { get; set; }
        [JsonPropertyName("from_district_id")]
        public int FromDistrictId { get; set; }
        [JsonPropertyName("from_ward_code")]
        public string FromWardCode  { get; set; }
        [JsonPropertyName("service_type_id")]
        public int ServiceTypeId { get; set; }
        [JsonPropertyName("items")]
        public List<GHNItem> Items { get; set; } = new();
        [JsonPropertyName("weight")]
        public int Weight { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }


    }
}
