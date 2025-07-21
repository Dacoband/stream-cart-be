using Shared.Common.Domain.Bases;
using ShopService.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace ShopService.Domain.Entities
{
    public class ShopVoucher : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; private set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; private set; } = string.Empty;

        [Required]
        public VoucherType Type { get; private set; }

        [Required]
        public decimal Value { get; private set; }

        public decimal? MaxValue { get; private set; }

        public decimal MinOrderAmount { get; private set; }

        [Required]
        public DateTime StartDate { get; private set; }

        [Required]
        public DateTime EndDate { get; private set; }

        [Required]
        public int AvailableQuantity { get; private set; }

        public int UsedQuantity { get; private set; }

        public bool IsActive { get; private set; }

        // Foreign Key
        [Required]
        public Guid ShopId { get; private set; }

        // Navigation Property
        public virtual Shop Shop { get; private set; } = null!;

        protected ShopVoucher() : base() { }

        public ShopVoucher(
            Guid shopId,
            string code,
            string description,
            VoucherType type,
            decimal value,
            decimal minOrderAmount,
            DateTime startDate,
            DateTime endDate,
            int availableQuantity,
            decimal? maxValue = null) : base()
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Mã voucher không được để trống", nameof(code));

            if (value <= 0)
                throw new ArgumentException("Giá trị voucher phải lớn hơn 0", nameof(value));

            if (type == VoucherType.Percentage && value > 100)
                throw new ArgumentException("Voucher phần trăm không được vượt quá 100%", nameof(value));

            if (startDate >= endDate)
                throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc", nameof(startDate));

            if (availableQuantity <= 0)
                throw new ArgumentException("Số lượng voucher phải lớn hơn 0", nameof(availableQuantity));

            ShopId = shopId;
            Code = code.ToUpper().Trim();
            Description = description?.Trim() ?? string.Empty;
            Type = type;
            Value = value;
            MaxValue = maxValue;
            MinOrderAmount = minOrderAmount;
            StartDate = startDate;
            EndDate = endDate;
            AvailableQuantity = availableQuantity;
            UsedQuantity = 0;
            IsActive = true;
        }

        public void UpdateDescription(string description)
        {
            Description = description?.Trim() ?? string.Empty;
        }

        public void UpdateValue(decimal value, decimal? maxValue = null)
        {
            if (value <= 0)
                throw new ArgumentException("Giá trị voucher phải lớn hơn 0", nameof(value));

            if (Type == VoucherType.Percentage && value > 100)
                throw new ArgumentException("Voucher phần trăm không được vượt quá 100%", nameof(value));

            Value = value;
            if (maxValue.HasValue)
                MaxValue = maxValue.Value;
        }

        public void UpdateMinOrderAmount(decimal minOrderAmount)
        {
            if (minOrderAmount < 0)
                throw new ArgumentException("Giá trị đơn hàng tối thiểu không được âm", nameof(minOrderAmount));

            MinOrderAmount = minOrderAmount;
        }

        public void UpdateDates(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
                throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc", nameof(startDate));

            StartDate = startDate;
            EndDate = endDate;
        }

        public void UpdateQuantity(int availableQuantity)
        {
            if (availableQuantity <= 0)
                throw new ArgumentException("Số lượng voucher phải lớn hơn 0", nameof(availableQuantity));

            AvailableQuantity = availableQuantity;
        }

        public void UpdateBasicInfo(
            string? description = null,
            decimal? value = null,
            decimal? maxValue = null,
            decimal? minOrderAmount = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? availableQuantity = null,
            string? modifier = null)
        {
            if (!string.IsNullOrWhiteSpace(description))
                Description = description.Trim();

            if (value.HasValue && value.Value > 0)
            {
                if (Type == VoucherType.Percentage && value.Value > 100)
                    throw new ArgumentException("Voucher phần trăm không được vượt quá 100%");
                Value = value.Value;
            }

            if (maxValue.HasValue)
                MaxValue = maxValue.Value;

            if (minOrderAmount.HasValue && minOrderAmount.Value >= 0)
                MinOrderAmount = minOrderAmount.Value;

            if (startDate.HasValue)
                StartDate = startDate.Value;

            if (endDate.HasValue)
                EndDate = endDate.Value;

            if (startDate.HasValue && endDate.HasValue && startDate.Value >= endDate.Value)
                throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn ngày kết thúc");

            if (availableQuantity.HasValue && availableQuantity.Value > 0)
                AvailableQuantity = availableQuantity.Value;

            SetModifier(modifier ?? "System");
        }

        public void Use()
        {
            if (UsedQuantity >= AvailableQuantity)
                throw new InvalidOperationException("Voucher đã hết số lượng");

            if (!IsVoucherValid())
                throw new InvalidOperationException("Voucher không còn hiệu lực");

            UsedQuantity++;
        }

        public void UseVoucher(string? modifier = null)
        {
            Use();
            SetModifier(modifier ?? "System");
        }

        public void Activate(string? modifier = null)
        {
            IsActive = true;
            SetModifier(modifier ?? "System");
        }

        public void Deactivate(string? modifier = null)
        {
            IsActive = false;
            SetModifier(modifier ?? "System");
        }

        public void SoftDelete(string? modifier = null)
        {
            Delete(modifier);
        }

        public bool IsVoucherValid()
        {
            var now = DateTime.UtcNow;
            return IsActive &&
                   now >= StartDate &&
                   now <= EndDate &&
                   UsedQuantity < AvailableQuantity;
        }

        public bool CanApplyToOrder(decimal orderAmount)
        {
            return IsVoucherValid() && orderAmount >= MinOrderAmount;
        }

        public decimal CalculateDiscount(decimal orderAmount)
        {
            if (!CanApplyToOrder(orderAmount))
                return 0;

            decimal discount = Type == VoucherType.Percentage
                ? orderAmount * (Value / 100)
                : Value;

            if (MaxValue.HasValue && discount > MaxValue.Value)
                discount = MaxValue.Value;

            return Math.Min(discount, orderAmount);
        }

        public override bool IsValid()
        {
            return base.IsValid() &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   Value > 0 &&
                   StartDate < EndDate &&
                   AvailableQuantity > 0;
        }
    }
}