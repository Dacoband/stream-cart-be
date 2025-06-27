using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeliveryNoteEnum
{
    [JsonPropertyName("CHOTHUHANG")]
    CHOTHUHANG,

    [JsonPropertyName("CHOXEMHANGKHONGTHU")]
    CHOXEMHANGKHONGTHU,

    [JsonPropertyName("KHONGCHOXEMHANG")]
    KHONGCHOXEMHANG
}
