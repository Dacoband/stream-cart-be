using Shared.Common.Domain.Bases;
using ShopService.Domain.Enums;
using System;

namespace ShopService.Domain.Entities
{
    public class Shop : BaseEntity
    {

        public string ShopName { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string LogoURL { get; private set; } = string.Empty;
        public string CoverImageURL { get; private set; } = string.Empty;
        public string BusinessLicenseImageURL { get; private set; } = string.Empty;

        public decimal RatingAverage { get; private set; }
        public int TotalReview { get; private set; }

        public DateTime RegistrationDate { get; private set; }
        public ApprovalStatus ApprovalStatus { get; private set; }
        public DateTime? ApprovalDate { get; private set; }

        public string BankAccountNumber { get; private set; } = string.Empty;
        public string BankName { get; private set; } = string.Empty;
        public string TaxNumber { get; private set; } = string.Empty;

        public int TotalProduct { get; private set; }
        public decimal CompleteRate { get; private set; }
        public ICollection<ShopMembership> ShopMemberships { get; set; }


        public ShopStatus Status { get; private set; }
       

        protected Shop() : base() { }

        /// <summary>
        /// Khởi tạo một cửa hàng mới
        /// </summary>
        /// <param name="shopName">Tên cửa hàng</param>
        /// <param name="accountId">ID của tài khoản sở hữu cửa hàng</param>
        /// <param name="description">Mô tả cửa hàng</param>
        /// <param name="logoURL">Đường dẫn logo</param>
        /// <param name="coverImageURL">Đường dẫn ảnh bìa</param>
        public Shop(
            string shopName,
            string? description = null,
            string? logoURL = null,
            string? coverImageURL = null,
            string? businessLicenseImageURL = null) : base()
        {
            if (string.IsNullOrWhiteSpace(shopName))
                throw new ArgumentException("Tên cửa hàng không được để trống", nameof(shopName));

            ShopName = shopName;
            Description = description ?? string.Empty;
            LogoURL = logoURL ?? string.Empty;
            CoverImageURL = coverImageURL ?? string.Empty;
            BusinessLicenseImageURL = businessLicenseImageURL ?? string.Empty;

            RatingAverage = 0;
            TotalReview = 0;

            RegistrationDate = DateTime.UtcNow;
            ApprovalStatus = ApprovalStatus.Pending;

            TotalProduct = 0;
            CompleteRate = 0;

            Status = ShopStatus.Inactive;
        }
        public void UpdateBasicInfo(
            string? shopName = null,
            string? description = null,
            string? logoURL = null,
            string? coverImageURL = null,
            string? businessLicenseImageURL = null,
            string? modifier = null)
        {
            if (!string.IsNullOrWhiteSpace(shopName))
                ShopName = shopName;

            Description = description ?? Description;
            LogoURL = logoURL ?? LogoURL;
            CoverImageURL = coverImageURL ?? CoverImageURL;
            BusinessLicenseImageURL = businessLicenseImageURL ?? BusinessLicenseImageURL;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        public void UpdateBankingInfo(
            string? bankAccountNumber = null,
            string? bankName = null,
            string? taxNumber = null,
            string? modifier = null)
        {
            BankAccountNumber = !string.IsNullOrWhiteSpace(bankAccountNumber) 
                ? bankAccountNumber 
                : BankAccountNumber;

            BankName = !string.IsNullOrWhiteSpace(bankName) 
                ? bankName 
                : BankName;

            TaxNumber = !string.IsNullOrWhiteSpace(taxNumber) 
                ? taxNumber 
                : TaxNumber;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Thêm đánh giá mới
        /// </summary>
        /// <param name="rating">Điểm đánh giá (0-5)</param>
        /// <param name="modifier">Người thực hiện thay đổi</param>
        /// <exception cref="ArgumentOutOfRangeException">Khi rating không hợp lệ</exception>
        public void AddReview(decimal rating, string? modifier = null)
        {
            if (rating < 0 || rating > 5)
                throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 5");

            decimal totalRatingPoints = RatingAverage * TotalReview;
            TotalReview++;
            RatingAverage = Math.Round((totalRatingPoints + rating) / TotalReview, 2);

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Cập nhật số lượng sản phẩm
        /// </summary>
        /// <param name="totalProduct">Tổng số sản phẩm</param>
        /// <param name="modifier">Người thực hiện thay đổi</param>
        public void UpdateProductCount(int totalProduct, string? modifier = null)
        {
            if (totalProduct >= 0)
                TotalProduct = totalProduct;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Cập nhật tỷ lệ hoàn thành
        /// </summary>
        /// <param name="completeRate">Tỷ lệ hoàn thành (0-100)</param>
        /// <param name="modifier">Người thực hiện thay đổi</param>
        /// <exception cref="ArgumentOutOfRangeException">Khi tỷ lệ không hợp lệ</exception>
        public void UpdateCompleteRate(decimal completeRate, string? modifier = null)
        {
            if (completeRate < 0 || completeRate > 100)
                throw new ArgumentOutOfRangeException(nameof(completeRate), "Complete rate must be between 0 and 100");

            CompleteRate = completeRate;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Phê duyệt cửa hàng
        /// </summary>
        /// <param name="modifier">Người thực hiện phê duyệt</param>
        public void Approve(string? modifier = null)
        {
            ApprovalStatus = ApprovalStatus.Approved;
            ApprovalDate = DateTime.UtcNow;
            Status = ShopStatus.Active;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Từ chối phê duyệt cửa hàng
        /// </summary>
        /// <param name="modifier">Người thực hiện từ chối</param>
        /// <param name="reason">Lý do từ chối</param>
        public void Reject(string? modifier = null, string? reason = null)
        {
            ApprovalStatus = ApprovalStatus.Rejected;
            ApprovalDate = DateTime.UtcNow;
            Status = ShopStatus.Inactive;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Kích hoạt cửa hàng
        /// </summary>
        /// <param name="modifier">Người thực hiện kích hoạt</param>
        public void Activate(string? modifier = null)
        {
            Status = ShopStatus.Active;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Vô hiệu hóa cửa hàng
        /// </summary>
        /// <param name="modifier">Người thực hiện vô hiệu hóa</param>
        public void Deactivate(string? modifier = null)
        {
            Status = ShopStatus.Inactive;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Kiểm tra tính hợp lệ của cửa hàng
        /// </summary>
        /// <returns>true nếu hợp lệ, false nếu không hợp lệ</returns>
        public override bool IsValid()
        {
            return base.IsValid() && !string.IsNullOrWhiteSpace(ShopName);
        }
        public void Pending(string? modifier = null)
        {
            ApprovalStatus = ApprovalStatus.Pending;
            ApprovalDate = DateTime.UtcNow;
            Status = ShopStatus.Inactive;

            if (!string.IsNullOrWhiteSpace(modifier))
                SetModifier(modifier);
            else
                LastModifiedAt = DateTime.UtcNow;
        }
    }
}