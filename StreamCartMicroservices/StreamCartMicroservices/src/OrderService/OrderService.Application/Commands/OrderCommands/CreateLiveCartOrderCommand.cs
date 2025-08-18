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
    }
}