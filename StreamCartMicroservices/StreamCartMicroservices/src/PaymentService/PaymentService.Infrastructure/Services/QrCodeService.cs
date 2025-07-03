using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Enums;
using ProductService.Domain.Enums;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QrCodeService> _logger;
        private readonly string _bankAccount;
        private readonly string _bankName;
        private readonly string _secretKey;

        public QrCodeService(IConfiguration configuration, ILogger<QrCodeService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Đọc thông tin ngân hàng từ cấu hình
            _bankAccount = _configuration["Payment:SePay:BankAccount"] ?? "0343219324";
            _bankName = _configuration["Payment:SePay:BankName"] ?? "MB";
            _secretKey = _configuration["Payment:QrCode:SecretKey"] ?? "DefaultSecretKey";
        }

        public async Task<string> GenerateQrCodeAsync(Guid orderId, decimal amount, Guid userId, PaymentMethod paymentMethod)
        {
            try
            {
                if (paymentMethod != PaymentMethod.BankTransfer)
                {
                    _logger.LogWarning("QR code can only be generated for bank transfers");
                    return string.Empty;
                }

                // Format mô tả theo yêu cầu của SePay
                string description = $"ORDER_{orderId:N}";

                // Làm tròn số tiền
                int amountInt = (int)amount;

                // Tham số QR code
                string template = "compact";
                string download = "true";

                // Tạo URL QR code từ SePay
                string qrUrl = $"https://qr.sepay.vn/img?acc={_bankAccount}&bank={_bankName}" +
                               $"&amount={amountInt}&des={description}&template={template}&download={download}";

                // Tạo signature để xác thực khi callback
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                string contentToSign = $"{orderId}|{amount}|{userId}|{timestamp}";
                string signature = await GenerateSignatureAsync(contentToSign);

                // Lưu thông tin bổ sung vào metadata (để xác thực khi callback)
                string metadata = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{contentToSign}|{signature}"));

                // Lưu URL QR code và metadata
                string qrCodeInfo = $"{qrUrl}|{metadata}";

                _logger.LogInformation("Generated SePay QR code for order {OrderId}", orderId);
                return qrCodeInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SePay QR code for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> ValidateQrCodeAsync(string qrCode)
        {
            try
            {
                // Tách URL QR code và metadata
                string[] qrParts = qrCode.Split('|', 2);
                if (qrParts.Length < 2)
                {
                    _logger.LogWarning("Invalid QR code format");
                    return false;
                }

                string metadata = qrParts[1];

                // Giải mã metadata
                string metadataContent = Encoding.UTF8.GetString(Convert.FromBase64String(metadata));
                string[] parts = metadataContent.Split('|');

                if (parts.Length < 5) // orderId, amount, userId, timestamp, signature
                {
                    _logger.LogWarning("Invalid QR code metadata format");
                    return false;
                }

                // Trích xuất thông tin
                string orderId = parts[0];
                string amount = parts[1];
                string userId = parts[2];
                string timestamp = parts[3];
                string providedSignature = parts[4];

                // Tạo lại nội dung để kiểm tra chữ ký
                string contentToVerify = $"{orderId}|{amount}|{userId}|{timestamp}";
                string expectedSignature = await GenerateSignatureAsync(contentToVerify);

                // Kiểm tra chữ ký
                bool isSignatureValid = providedSignature == expectedSignature;

                // Kiểm tra hết hạn (30 phút)
                long timestampValue = long.Parse(timestamp);
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long qrCodeLifetime = 30 * 60; // 30 phút

                bool isNotExpired = currentTime - timestampValue <= qrCodeLifetime;

                return isSignatureValid && isNotExpired;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating QR code");
                return false;
            }
        }

        private Task<string> GenerateSignatureAsync(string content)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
            byte[] signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Task.FromResult(Convert.ToHexString(signatureBytes));
        }
    }
}