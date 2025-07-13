using MediatR;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Interfaces;
using ShopService.Application.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using Shared.Common.Services.Email;

namespace ShopService.Application.Handlers
{
    public class RejectShopCommandHandler : IRequestHandler<RejectShopCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IEmailService _emailService;
        private readonly IAccountServiceClient _accountServiceClient;

        public RejectShopCommandHandler(IShopRepository shopRepository, IMessagePublisher messagePublisher, IEmailService emailService, IAccountServiceClient accountServiceClient)
        {
            _shopRepository = shopRepository;
            _messagePublisher = messagePublisher;
            _emailService = emailService;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<ShopDto> Handle(RejectShopCommand request, CancellationToken cancellationToken)
        {
            // Tìm shop theo ID
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());
            if (shop == null)
            {
                throw new ArgumentException($"Shop with ID {request.ShopId} not found");
            }

            // Set người reject
            if (!string.IsNullOrEmpty(request.RejectedBy))
            {
                shop.SetModifier(request.RejectedBy);
            }

            // Reject shop
            shop.Reject(request.RejectedBy, request.RejectionReason);

            // Lưu thay đổi
            await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

            // Tạo và publish event thông báo shop đã bị reject
            await _messagePublisher.PublishAsync(new ShopRejected
            {
                ShopId = shop.Id,
                ShopName = shop.ShopName,
                Reason = request.RejectionReason,
                RejectionDate = DateTime.UtcNow
            }, cancellationToken);
            //Send mail 
            var account = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(shop.CreatedBy));
            var htmlBody = $@"
<div style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
  <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='text-align: center; color: #e74c3c;'>❌ Từ chối đăng ký shop trên StreamCart</h2>
    <p>Xin chào <strong>{account.Fullname ?? account.Username}</strong>,</p>
    <p>Rất tiếc, shop của bạn đã <strong>không được phê duyệt</strong> để hoạt động trên nền tảng <span style='color: #007bff; font-weight: bold;'>StreamCart</span> vào thời điểm này.</p>
    <p>Lý do từ chối:</p>
    <blockquote style='background-color: #fcebea; color: #cc1f1a; padding: 15px; border-left: 4px solid #e3342f; border-radius: 4px;'>
      {request.RejectionReason}
    </blockquote>
    <p>Nếu bạn cần thêm thông tin hoặc muốn điều chỉnh và đăng ký lại, vui lòng liên hệ với đội ngũ hỗ trợ của chúng tôi.</p>
    <p style='margin-top: 30px;'>Trân trọng,<br/>Đội ngũ <strong>StreamCart</strong></p>
  </div>
</div>";

            await _emailService.SendEmailAsync(
                account.Email,
                "Thông báo từ chối đăng ký shop trên StreamCart",
                htmlBody,
                account.Fullname);
            // Trả về DTO
            return new ShopDto
            {
                Id = shop.Id,
                ShopName = shop.ShopName,
                Description = shop.Description,
                LogoURL = shop.LogoURL,
                CoverImageURL = shop.CoverImageURL,
                RatingAverage = shop.RatingAverage,
                TotalReview = shop.TotalReview,
                RegistrationDate = shop.RegistrationDate,
                ApprovalStatus = shop.ApprovalStatus.ToString(),
                ApprovalDate = shop.ApprovalDate,
                BankAccountNumber = shop.BankAccountNumber,
                BankName = shop.BankName,
                TaxNumber = shop.TaxNumber,
                TotalProduct = shop.TotalProduct,
                CompleteRate = shop.CompleteRate,
                Status = shop.Status == ShopService.Domain.Enums.ShopStatus.Active,
                CreatedAt = shop.CreatedAt,
                CreatedBy = shop.CreatedBy,
                LastModifiedAt = shop.LastModifiedAt,
                LastModifiedBy = shop.LastModifiedBy
            };
        }
    }
}