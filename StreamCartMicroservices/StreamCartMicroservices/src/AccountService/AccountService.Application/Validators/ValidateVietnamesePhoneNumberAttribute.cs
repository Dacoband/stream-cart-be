using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AccountService.Application.Validators
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateVietnamesePhoneNumberAttribute : ValidationAttribute
    {
        public ValidateVietnamesePhoneNumberAttribute()
        {
            ErrorMessage = "Vui lòng nhập số điện thoại";
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
                return false; // PhoneNumber bắt buộc

            string phoneNumber = value.ToString() ?? string.Empty;

            // Số điện thoại Việt Nam:
            // - Bắt đầu bằng số 0
            // - Theo sau là 9-11 chữ số
            // - Tổng độ dài 10-12 chữ số
            // - Các đầu số hợp lệ: 03, 05, 07, 08, 09 (Viettel, Mobifone, Vinaphone, v.v.)
            string pattern = @"^(0)(3[2-9]|5[6|8|9]|7[0|6-9]|8[0-9]|9[0-9])[0-9]{7,9}$";

            return Regex.IsMatch(phoneNumber, pattern);
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult("Vui lòng nhập số điện thoại");

            string phoneNumber = value.ToString() ?? string.Empty;

            // Số điện thoại Việt Nam:
            // - Bắt đầu bằng số 0
            // - Theo sau là 9-11 chữ số
            // - Tổng độ dài 10-12 chữ số
            // - Các đầu số hợp lệ: 03, 05, 07, 08, 09 (Viettel, Mobifone, Vinaphone, v.v.)
            string pattern = @"^(0)(3[2-9]|5[6|8|9]|7[0|6-9]|8[0-9]|9[0-9])[0-9]{7,9}$";

            if (!Regex.IsMatch(phoneNumber, pattern))
                return new ValidationResult("Số điện thoại không hợp lệ");

            return ValidationResult.Success;
        }
    }
}
