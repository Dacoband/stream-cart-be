using MediatR;
using OrderService.Application.DTOs;
using Shared.Common.Models;

namespace OrderService.Application.Commands.OrderCommands
{
    public class CreateLiveCartOrderCommand : IRequest<ApiResponse<LivestreamOrderResult>>
    {
        public Guid UserId { get; set; }
        public Guid LivestreamId { get; set; }
        public List<LiveCartItemDto> CartItems { get; set; } = new();
        public string PaymentMethod { get; set; } = "COD";
        public Guid DeliveryAddressId { get; set; }
        public string? CustomerNotes { get; set; }
        public string? VoucherCode { get; set; }
        public Guid? CreatedFromCommentId { get; set; }
        public Guid? ShippingProviderId { get; set; }
        public decimal? ShippingFee { get; set; }
        public DateTime? ExpectedDeliveryDay { get; set; }
    }
}