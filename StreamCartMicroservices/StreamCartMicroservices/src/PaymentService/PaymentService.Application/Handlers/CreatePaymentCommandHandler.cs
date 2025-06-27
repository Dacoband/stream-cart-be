using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Commands;
using PaymentService.Application.DTOs;
using PaymentService.Application.Events;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using Shared.Common.Services.User;

namespace PaymentService.Application.Handlers
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAccountServiceClient _accountServiceClient;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<CreatePaymentCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreatePaymentCommandHandler(
            IPaymentRepository paymentRepository,
            IMessagePublisher messagePublisher,
            IAccountServiceClient accountServiceClient,
            ILogger<CreatePaymentCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _paymentRepository = paymentRepository;
            _messagePublisher = messagePublisher;
            _accountServiceClient = accountServiceClient;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Guid userId = _currentUserService.GetUserId();
                bool userExists = await _accountServiceClient.DoesUserExistAsync(request.UserId);
                if (!userExists)
                {
                    throw new ApplicationException($"Tài khoản với ID {request.UserId} không tồn tại");
                }
                // Create new payment entity
                var payment = new Payment(
                    request.OrderId,
                    request.UserId,
                    request.Amount,
                    request.PaymentMethod);

                if (!string.IsNullOrEmpty(request.CreatedBy))
                    payment.SetCreator(request.CreatedBy);

                // Validate payment
                if (!payment.IsValid())
                {
                    throw new ArgumentException("Payment data is invalid");
                }

                // Save to database
                await _paymentRepository.InsertAsync(payment);

                // Bổ sung try-catch với timeout cho phần publish message
                try
                {
                    var publishTask = _messagePublisher.PublishAsync(new PaymentCreated
                    {
                        PaymentId = payment.Id,
                        OrderId = payment.OrderId,
                        UserId = payment.UserId,
                        Amount = payment.Amount,
                        PaymentMethod = payment.PaymentMethod.ToString(),
                        CreatedAt = payment.CreatedAt
                    }, cancellationToken);

                    // Đặt timeout để tránh treo ứng dụng
                    var timeoutTask = Task.Delay(5000); // 5 giây timeout

                    if (await Task.WhenAny(publishTask, timeoutTask) == timeoutTask)
                    {
                        _logger.LogWarning("Publish payment event timeout. Payment ID: {PaymentId}", payment.Id);
                        // Vẫn tiếp tục thực thi mà không chờ publish
                    }
                    else
                    {
                        // Hoàn thành task publish
                        await publishTask;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi publish thông báo payment created. PaymentId: {PaymentId}", payment.Id);
                    // Vẫn tiếp tục thực thi mà không throw exception
                }

                _logger.LogInformation("Created new payment {PaymentId} for order {OrderId}", payment.Id, payment.OrderId);

                // Return DTO
                return new PaymentDto
                {
                    Id = payment.Id,
                    OrderId = payment.OrderId,
                    UserId = payment.UserId,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod.ToString(),
                    Status = payment.Status.ToString(),
                    QrCode = payment.QrCode,
                    Fee = payment.Fee,
                    ProcessedAt = payment.ProcessedAt,
                    CreatedAt = payment.CreatedAt,
                    CreatedBy = payment.CreatedBy,
                    LastModifiedAt = payment.LastModifiedAt,
                    LastModifiedBy = payment.LastModifiedBy
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for order {OrderId}", request.OrderId);
                throw;
            }
        }
    }
}