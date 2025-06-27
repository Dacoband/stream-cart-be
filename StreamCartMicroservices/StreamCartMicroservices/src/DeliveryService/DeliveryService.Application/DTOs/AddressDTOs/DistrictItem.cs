using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.AddressDTOs
{
    public class DistrictItem
    {
        [JsonPropertyName("DistrictID")]

        public int DistrictId { get; set; } 
        public string DistrictName { get; set; }
    }
}
