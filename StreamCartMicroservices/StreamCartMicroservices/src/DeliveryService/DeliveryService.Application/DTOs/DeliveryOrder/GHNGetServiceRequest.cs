using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.DeliveryOrder
{
    public class GHNGetServiceRequest
    {
        [JsonPropertyName("to_district")]
        public int ToDistrictId { get; set; }
        [JsonPropertyName("from_district")]
        public int FromDistrictId { get; set; }
    }
}
