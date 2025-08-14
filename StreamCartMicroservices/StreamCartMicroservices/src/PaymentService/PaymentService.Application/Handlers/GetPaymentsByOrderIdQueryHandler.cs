using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MediatR;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Queries;

namespace PaymentService.Application.Handlers
{
    public class GetPaymentsByOrderIdQueryHandler : IRequestHandler<GetPaymentsByOrderIdQuery, IEnumerable<PaymentDto>>
    {
        private readonly IPaymentRepository _paymentRepository;

        public GetPaymentsByOrderIdQueryHandler(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<IEnumerable<PaymentDto>> Handle(GetPaymentsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            // Sử dụng GetByOrderIdAsync thay vì GetPaymentsByOrderIdAsync
            var payments = await _paymentRepository.GetByOrderIdAsync(request.OrderId);

            // Manual mapping thay vì sử dụng AutoMapper
            return payments.Select(p => new PaymentDto
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
            });
        }
    }
}