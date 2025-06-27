using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries;
using Shared.Common.Domain.Bases;

namespace PaymentService.Application.Handlers
{
    public class GetPagedPaymentsQueryHandler : IRequestHandler<GetPagedPaymentsQuery, PagedResult<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<GetPagedPaymentsQueryHandler> _logger;

        public GetPagedPaymentsQueryHandler(
            IPaymentRepository paymentRepository,
            ILogger<GetPagedPaymentsQueryHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        public async Task<PagedResult<PaymentDto>> Handle(GetPagedPaymentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pagedResult = await _paymentRepository.GetPagedPaymentsAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.Status,
                    request.Method,
                    request.UserId,
                    request.OrderId,
                    request.FromDate,
                    request.ToDate,
                    request.SortBy,
                    request.Ascending);

                var paymentDtos = pagedResult.Items.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    Status = p.Status.ToString(),
                    QrCode = p.QrCode,
                    Fee = p.Fee,
                    ProcessedAt = p.ProcessedAt,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    LastModifiedAt = p.LastModifiedAt,
                    LastModifiedBy = p.LastModifiedBy
                }).ToList();

                return new PagedResult<PaymentDto>
                {
                    Items = paymentDtos,
                    CurrentPage = pagedResult.CurrentPage,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged payments");
                throw;
            }
        }
    }
}