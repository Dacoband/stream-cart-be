using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeliveryService.Application.DTOs.AddressDTOs
{
    public class WardItem
    {
        [JsonPropertyName("WardCode")]
        public string WardCode { get; set; }

        [JsonPropertyName("WardName")]
        public string WardName { get; set; }


    }
}
