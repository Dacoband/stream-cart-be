using MediatR;
using OrderService.Application.DTOs.RefundDTOs;
using OrderService.Domain.Enums;
using System;

namespace OrderService.Application.Commands.RefundCommands
{
    public class UpdateRefundStatusCommand : IRequest<RefundRequestDto>
    {
        public Guid RefundRequestId { get; set; }
        public RefundStatus NewStatus { get; set; }
        //public string ModifiedBy { get; set; } = string.Empty;
        //public string? TrackingCode { get; set; }
    }
}