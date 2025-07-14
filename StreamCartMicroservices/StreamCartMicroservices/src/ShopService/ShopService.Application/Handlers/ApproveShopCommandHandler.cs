using MediatR;
using Shared.Common.Services.Email;
using ShopService.Application.Commands;
using ShopService.Application.DTOs;
using ShopService.Application.Events;
using ShopService.Application.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ShopService.Application.Handlers
{
    public class ApproveShopCommandHandler : IRequestHandler<ApproveShopCommand, ShopDto>
    {
        private readonly IShopRepository _shopRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IEmailService _emailService;
        private readonly IAccountServiceClient _accountServiceClient;
        public ApproveShopCommandHandler(IShopRepository shopRepository, IMessagePublisher messagePublisher, IEmailService emailService, IAccountServiceClient accountServiceClient)
        {
            _shopRepository = shopRepository;
            _messagePublisher = messagePublisher;
            _emailService = emailService;
            _accountServiceClient = accountServiceClient;
        }

        public async Task<ShopDto> Handle(ApproveShopCommand request, CancellationToken cancellationToken)
        {
            // Get shop by ID
            var shop = await _shopRepository.GetByIdAsync(request.ShopId.ToString());
            if (shop == null)
            {
                throw new ArgumentException($"Shop with ID {request.ShopId} not found");
            }

            // Set approver
            if (!string.IsNullOrEmpty(request.ApprovedBy))
            {
                shop.SetModifier(request.ApprovedBy);
            }

            // Approve shop
            shop.Approve();

            // Save changes
            await _shopRepository.ReplaceAsync(shop.Id.ToString(), shop);

            // Publish event
            await _messagePublisher.PublishAsync(new ShopApproved
            {
                ShopId = shop.Id,
                ShopName = shop.ShopName,
                AccountId = Guid.Empty, 
                ApprovalDate = shop.ApprovalDate ?? DateTime.UtcNow
            }, cancellationToken);
            //Send mail 
            var account = await _accountServiceClient.GetAccountByAccountIdAsync(Guid.Parse(shop.CreatedBy));
            var htmlBody = $@"
<div style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
  <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='text-align: center; color: #2c3e50;'>🎉 Xác nhận phê duyệt shop</h2>
    <p>Xin chào <strong>{account.Fullname ?? account.Username}</strong>,</p>
    <p>Chúc mừng! Shop của bạn đã được <strong>phê duyệt thành công</strong> trên nền tảng thương mại điện tử livestream <span style='color: #007bff; font-weight: bold;'>StreamCart</span>.</p>
    <p>Từ nay, bạn có thể:</p>
    <ul>
      <li>Đăng bán sản phẩm trực tiếp qua livestream</li>
      <li>Quản lý đơn hàng, sản phẩm và khách hàng một cách dễ dàng</li>
      <li>Tăng doanh thu nhờ nền tảng hỗ trợ bán hàng hiệu quả</li>
    </ul>
    <p>Hãy truy cập trang quản trị của bạn để bắt đầu:</p>
    <div style='text-align: center; margin-top: 20px;'>
      <a href='https://admin.streamcart.vn' style='background-color: #007bff; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px;'>Bắt đầu ngay</a>
    </div>
    <p style='margin-top: 30px;'>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi qua email hoặc fanpage hỗ trợ.</p>
    <p>Trân trọng,<br/>Đội ngũ <strong>StreamCart</strong></p>
  </div>
</div>";

            await _emailService.SendEmailAsync(account.Email, "Cửa hàng của bạn đã được phê duyệt trên StreamCart!", htmlBody, account.Fullname);
            // Return DTO
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