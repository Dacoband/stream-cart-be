using System.Text.Json.Serialization;

namespace DeliveryService.Application.DTOs.DeliveryOrder
{
    public class GHNCreateOrderRequest
    {

        [JsonPropertyName("to_name")]
        public string ToName { get; set; }

        [JsonPropertyName("to_phone")]
        public string ToPhone { get; set; }
        [JsonPropertyName("to_province")]
        public string? ToProvince { get; set; }

        [JsonPropertyName("to_address")]
        public string ToAddress { get; set; }

        [JsonPropertyName("to_ward_code")]
        public string ToWardCode { get; set; }

        [JsonPropertyName("to_district_id")]
        public int ToDistrictId { get; set; }

        [JsonPropertyName("from_name")]
        public string FromName { get; set; }

        [JsonPropertyName("from_phone")]
        public string FromPhone { get; set; }

        [JsonPropertyName("from_address")]
        public string FromAddress { get; set; }

        [JsonPropertyName("from_ward_name")]
        public string FromWardName { get; set; }

        [JsonPropertyName("from_district_name")]
        public string FromDistrictName { get; set; }

        [JsonPropertyName("from_province_name")]
        public string FromProvinceName { get; set; }

        [JsonPropertyName("weight")]
        public int Weight { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("service_type_id")]
        public int ServiceTypeId { get; set; }

        [JsonPropertyName("payment_type_id")]
        public int PaymentTypeId { get; set; } = 1;

        [JsonPropertyName("required_note")]

        public DeliveryNoteEnum RequiredNote { get; set; } = DeliveryNoteEnum.KHONGCHOXEMHANG;

        [JsonPropertyName("items")]
        public List<GHNItem> Items { get; set; } = new();


        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("return_phone")]
        public string? ReturnPhone { get; set; }

        [JsonPropertyName("return_address")]
        public string? ReturnAddress { get; set; }

        [JsonPropertyName("return_district_id")]
        public int? ReturnDistrictId { get; set; }

        [JsonPropertyName("return_ward_code")]
        public string? ReturnWardCode { get; set; }


        [JsonPropertyName("content")]
        public string? Description { get; set; }

        [JsonPropertyName("cod_amount")]
        public int? CodAmount { get; set; }

       
    }

    public class GHNItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }        

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }       

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
