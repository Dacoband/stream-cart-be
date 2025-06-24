using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Commands;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries;
using PaymentService.Domain.Enums;
using ProductService.Domain.Enums;
using Shared.Common.Domain.Bases;

namespace PaymentService.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IMediator mediator, ILogger<PaymentService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto createPaymentDto)
        {
            try
            {
                var command = new CreatePaymentCommand
                {
                    OrderId = createPaymentDto.OrderId,
                    UserId = createPaymentDto.UserId,
                    Amount = createPaymentDto.Amount,
                    PaymentMethod = createPaymentDto.PaymentMethod,
                    CreatedBy = createPaymentDto.CreatedBy
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePaymentAsync for order {OrderId}", createPaymentDto.OrderId);
                throw;
            }
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(Guid paymentId)
        {
            try
            {
                var query = new GetPaymentByIdQuery { PaymentId = paymentId };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentByIdAsync for payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<PagedResult<PaymentDto>> GetPagedPaymentsAsync(
            int pageNumber,
            int pageSize,
            PaymentStatus? status = null,
            PaymentMethod? method = null,
            Guid? userId = null,
            Guid? orderId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? sortBy = null,
            bool ascending = true)
        {
            try
            {
                var query = new GetPagedPaymentsQuery
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Status = status,
                    Method = method,
                    UserId = userId,
                    OrderId = orderId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    SortBy = sortBy,
                    Ascending = ascending
                };

                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPagedPaymentsAsync");
                throw;
            }
        }

        public async Task<PaymentDto?> UpdatePaymentStatusAsync(Guid paymentId, UpdatePaymentStatusDto updateStatusDto)
        {
            try
            {
                var command = new UpdatePaymentStatusCommand
                {
                    PaymentId = paymentId,
                    NewStatus = updateStatusDto.NewStatus,
                    QrCode = updateStatusDto.QrCode,  
                    Fee = updateStatusDto.Fee,
                    UpdatedBy = updateStatusDto.UpdatedBy
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdatePaymentStatusAsync for payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<PaymentDto?> ProcessPaymentCallbackAsync(Guid paymentId, PaymentCallbackDto callbackDto)
        {
            try
            {
                var command = new ProcessPaymentCallbackCommand
                {
                    PaymentId = paymentId,
                    IsSuccessful = callbackDto.IsSuccessful,
                    QrCode = callbackDto.QrCode,
                    Fee = callbackDto.Fee,
                    ErrorMessage = callbackDto.ErrorMessage,
                    RawResponse = callbackDto.RawResponse
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPaymentCallbackAsync for payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<PaymentDto?> RefundPaymentAsync(Guid paymentId, RefundPaymentDto refundDto)
        {
            try
            {
                var command = new RefundPaymentCommand
                {
                    PaymentId = paymentId,
                    Reason = refundDto.Reason,
                    RequestedBy = refundDto.RequestedBy
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefundPaymentAsync for payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(Guid userId)
        {
            try
            {
                var query = new GetPaymentsByUserIdQuery { UserId = userId };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentsByUserIdAsync for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(Guid orderId)
        {
            try
            {
                var query = new GetPaymentsByOrderIdQuery { OrderId = orderId };
                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentsByOrderIdAsync for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> DeletePaymentAsync(Guid paymentId, string deletedBy)
        {
            try
            {
                var command = new DeletePaymentCommand
                {
                    PaymentId = paymentId,
                    DeletedBy = deletedBy
                };

                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeletePaymentAsync for payment {PaymentId}", paymentId);
                throw;
            }
        }

        public async Task<PaymentSummaryDto> GetPaymentSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = new GetPaymentSummaryQuery
                {
                    FromDate = fromDate,
                    ToDate = toDate
                };

                return await _mediator.Send(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaymentSummaryAsync");
                throw;
            }
        }
    }
}