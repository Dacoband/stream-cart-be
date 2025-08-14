using MediatR;
using ProductService.Application.Commands.ProductComands;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Interfaces;
using Shared.Common.Services.Email;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.Application.Handlers.ProductHandlers
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;
        private readonly IAccountCLientService _accountCLientService;
        private readonly IEmailService _emailService;
        private readonly IShopServiceClient _shopServiceClient;

        public DeleteProductCommandHandler(IProductRepository productRepository, IEmailService emailService, IShopServiceClient shopServiceClient)
        {
            _productRepository = productRepository;
            _emailService = emailService;
            _shopServiceClient = shopServiceClient; 
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id.ToString());


            if (product == null)
            {
                return false;
            }

            // Thực hiện xóa mềm
            product.Delete(request.DeletedBy);

            if (!string.IsNullOrEmpty(request.DeletedBy))
            {
                product.SetUpdatedBy(request.DeletedBy);
            }
            
            var deletedBy = await _accountCLientService.GetAccountById(request.Id.ToString());
            if(deletedBy.Role == "OperationManager")
            {
                var shop =await _shopServiceClient.GetShopByIdAsync((Guid)product.ShopId);
                var seller = await _accountCLientService.GetAccountById(shop.CreatedBy);
                var htmlBody = $@"
<div style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
  <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
    <h2 style='text-align: center; color: #e74c3c;'>⚠️ Thông báo sản phẩm bị xóa</h2>
    <p>Xin chào <strong>{seller.Fullname ?? seller.Username}</strong>,</p>
    <p>Sản phẩm <strong>{product.ProductName}</strong> của bạn đã bị <strong>xóa</strong> khỏi hệ thống StreamCart.</p>
    <p><strong>Lý do:</strong> {request.Reason}</p>
    <p>Vui lòng kiểm tra lại thông tin sản phẩm và đảm bảo tuân thủ các chính sách đăng bán của nền tảng.</p>
    <p>Nếu bạn cho rằng đây là nhầm lẫn, hãy liên hệ với bộ phận hỗ trợ để được xem xét lại.</p>
    <div style='text-align: center; margin-top: 20px;'>
      <a href='https://admin.streamcart.vn' style='background-color: #007bff; color: #ffffff; padding: 12px 24px; text-decoration: none; border-radius: 6px;'>Đăng nhập quản trị</a>
    </div>
    <p style='margin-top: 30px;'>Cảm ơn bạn đã hợp tác và tuân thủ quy định của StreamCart.</p>
    <p>Trân trọng,<br/>Đội ngũ <strong>StreamCart</strong></p>
  </div>
</div>";

                await _emailService.SendEmailAsync(seller.Email, "Thông báo sản phẩm bị xóa khỏi StreamCart", htmlBody, seller.Fullname);
            }
            await _productRepository.ReplaceAsync(product.Id.ToString(), product);

            return true;
        }
    }
}