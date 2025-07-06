using OrderService.Application.DTOs.WalletDTOs;
using System.Threading.Tasks;

namespace OrderService.Application.Interfaces
{
    /// <summary>
    /// Giao diện kết nối với dịch vụ ví điện tử
    /// </summary>
    public interface IWalletServiceClient
    {
        /// <summary>
        /// Xử lý thanh toán cho shop sau khi đơn hàng hoàn thành
        /// </summary>
        /// <param name="paymentRequest">Thông tin yêu cầu thanh toán</param>
        Task ProcessShopPaymentAsync(ShopPaymentRequest paymentRequest);
    }
}