using AccountService.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Domain.Entities
{
    public class Address : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string RecipientName { get; private set; }
        [Required]
        [StringLength(100)]
        public string Street { get; private set; }
        [StringLength(100)]
        public string Ward { get; private set; } = string.Empty;
        [StringLength(100)]
        public string District { get; private set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string City { get; private set; }
        [Required]
        [StringLength(100)]
        public string Country { get; private set; }
        [StringLength(20)]
        public string PostalCode { get; private set; } = string.Empty;
        [StringLength(20)]
        public string PhoneNumber { get; private set; } = string.Empty;
        [Required]
        public bool IsDefaultShipping { get; private set; }
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }
        [Required] // Loại địa chỉ, mặc định là Shipping
        public AddressType Type { get; private set; } = AddressType.Shipping;
        [Required]
        public bool IsActive { get; private set; } = true;
        [ForeignKey("Account")]
        public Guid AccountId { get; private set; }
        [ForeignKey("Shop")]
        public Guid? ShopId { get; private set; }
        private Address() { }
        public Address(
            Guid accountId,
            string recipientName,
            string street,
            string city,
            string country,
            string phoneNumber,
            AddressType type = AddressType.Shipping,
            string createdBy = "system") : base()
        {
            if (string.IsNullOrWhiteSpace(recipientName))
                throw new ArgumentException("Recipient name is required", nameof(recipientName));

            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street is required", nameof(street));

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City is required", nameof(city));

            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Country is required", nameof(country));

            AccountId = accountId;
            RecipientName = recipientName;
            Street = street;
            City = city;
            Country = country;
            PhoneNumber = phoneNumber;
            Type = type;
            IsDefaultShipping = false;
            IsActive = true;
            SetCreator(createdBy);
        }
        public void UpdateAddress(
           string recipientName,
           string street,
           string ward,
           string district,
           string city,
           string country,
           string postalCode,
           string phoneNumber)
        {
            if (!string.IsNullOrWhiteSpace(recipientName))
                RecipientName = recipientName;

            if (!string.IsNullOrWhiteSpace(street))
                Street = street;

            if (ward != null)
                Ward = ward;

            if (district != null)
                District = district;

            if (!string.IsNullOrWhiteSpace(city))
                City = city;

            if (!string.IsNullOrWhiteSpace(country))
                Country = country;

            if (postalCode != null)
                PostalCode = postalCode;

            if (!string.IsNullOrWhiteSpace(phoneNumber))
                PhoneNumber = phoneNumber;
        }
        public void AssignToShop(Guid shopId)
        {
            ShopId = shopId;
        }
        public void UnassignFromShop()
        {
            ShopId = null;
        }
        public void UpdateType(AddressType type)
        {
            Type = type;
        }
        public void SetAsDefaultShipping()
        {
            IsDefaultShipping = true;
        }
        public void UnsetAsDefaultShipping() // Hủy đặt làm địa chỉ giao hàng mặc định
        {
            IsDefaultShipping = false;
        }
        public void UpdateLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        public void SetUpdatedBy(string updatedBy)
        {
            LastModifiedBy = updatedBy;
            LastModifiedAt = DateTime.UtcNow;
        }
        public void Activate()
        {
            IsActive = true;
        }
        public void Deactivate()
        {
            IsActive = false;
        }
        public override bool IsValid()
        {
            if (!base.IsValid())
                return false;

            if (string.IsNullOrWhiteSpace(RecipientName))
                return false;

            if (string.IsNullOrWhiteSpace(Street))
                return false;

            if (string.IsNullOrWhiteSpace(City))
                return false;

            if (string.IsNullOrWhiteSpace(Country))
                return false;

            return true;
        }
    }
}

