using MediatR;
using OrderService.Application.DTOs.RefundDTOs;

namespace OrderService.Application.Commands.RefundCommands
{
    /// <summary>
    /// Command to update refund transaction ID
    /// </summary>
    public class UpdateRefundTransactionCommand : IRequest<RefundRequestDto>
    {
        public Guid RefundRequestId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}