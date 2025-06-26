using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.AddressDTOs
{
    public class GHNServicesDTO
    {
        [JsonPropertyName("service_type_id")]

        public int ServiceTypeId { get; set; }
        [JsonPropertyName("short_name")]

        public string ShortName { get; set; }
        
    }
}
