using AccountService.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AccountService.Application.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RoleValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return false;

            if (value is RoleType role)
            {
                return role == RoleType.Customer || role == RoleType.Seller;
            }

            return false;
        }
    }
}
