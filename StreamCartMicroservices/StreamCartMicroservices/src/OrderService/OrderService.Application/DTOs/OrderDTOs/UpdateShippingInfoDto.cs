namespace OrderService.Application.DTOs.OrderDTOs
{
    public class UpdateShippingInfoDto
    {
        public string ToName { get; set; } = string.Empty;
        public string ToPhone { get; set; } = string.Empty;
        public string ToAddress { get; set; } = string.Empty;
        public string ToWard { get; set; } = string.Empty;
        public string ToDistrict { get; set; } = string.Empty;
        public string ToProvince { get; set; } = string.Empty;
        public string ToPostalCode { get; set; } = string.Empty;
    }
}