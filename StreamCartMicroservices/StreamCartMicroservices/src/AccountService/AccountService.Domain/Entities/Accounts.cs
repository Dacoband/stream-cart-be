using AccountService.Domain.Enums;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccountService.Domain.Entities
{
    public class Account : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string Username { get; private set; }
        
        [Required]
        public string Password { get; private set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; private set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; private set; }
        
        [StringLength(100)]
        public string? Fullname { get; private set; }
        
        [StringLength(255)]
        public string? AvatarURL { get; private set; }
        
        [Required]
        public RoleType Role { get; private set; }
        
        public DateTime RegistrationDate { get; private set; }
        
        public DateTime? LastLoginDate { get; private set; }
        
        public bool IsActive { get; private set; } = true;
        
        public bool IsVerified { get; private set; } = false;
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? CompleteRate { get; private set; }
        [ForeignKey("Shop")]
        public Guid? ShopId { get; private set; }

        // public virtual Shop? Shop { get; private set; }

        // Thêm các thuộc tính cho xác thực OTP
        public string? VerificationToken { get; private set; }
        
        public DateTime? VerificationTokenExpiry { get; private set; }
        
        // Private constructor for EF Core
        private Account() { }
        
        // Constructor for creating new account
        public Account(
            string username,
            string password,
            string email,
            RoleType role) : base()
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));
                
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));
                
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
                
            if (string.IsNullOrWhiteSpace(role.ToString()))
                throw new ArgumentException("Role cannot be empty", nameof(role));
            
            Username = username;
            Password = password; 
            Email = email;
            Role = role;
            RegistrationDate = DateTime.UtcNow;
        }
        
        public void UpdateProfile(string? fullname, string? phoneNumber, string? avatarURL)
        {
            if (fullname != null)
                Fullname = fullname;
                
            if (phoneNumber != null)
                PhoneNumber = phoneNumber;
                
            if (avatarURL != null)
                AvatarURL = avatarURL;
        }
        
        public void UpdatePassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be empty", nameof(newPassword));
                
            Password = newPassword; 
        }
        
        public void ChangeRole(RoleType newRole)
        {
            Role = newRole;
        }
        
        public void SetVerified()
        {
            IsVerified = true;
        }
        
        public void RecordLogin()
        {
            LastLoginDate = DateTime.UtcNow;
        }
        
        public void SetShop(Guid shopId)
        {
            ShopId = shopId;
        }
        
        public void UpdateCompleteRate(decimal rate)
        {
            if (rate < 0 || rate > 100)
                throw new ArgumentOutOfRangeException(nameof(rate), "Complete rate must be between 0 and 100");
                
            CompleteRate = rate;
        }
        
        public void Activate()
        {
            IsActive = true;
        }
        
        public void Deactivate()
        {
            IsActive = false;
        }

        public void SetVerificationToken(string token)
        {
            VerificationToken = token;
        }

        public void SetVerificationTokenExpiry(DateTime expiry)
        {
            VerificationTokenExpiry = expiry;
        }

        public void ClearVerificationToken()
        {
            VerificationToken = null;
            VerificationTokenExpiry = null;
        }

        public void SetUnverified()
        {
            IsVerified = false;
        }

        public void UpdateRole(RoleType role)
        {
            Role = role;
        }

        public void UpdateShopId(Guid shopId)
        {
            ShopId = shopId;
        }

        public void SetUpdatedBy(string updatedBy)
        {
            this.SetModifier(updatedBy);
        }

        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;
                
            if (string.IsNullOrWhiteSpace(Username))
                return false;
                
            if (string.IsNullOrWhiteSpace(Email))
                return false;
                
            if (string.IsNullOrWhiteSpace(Password))
                return false;
                
            return true;
        }
    }
}   
