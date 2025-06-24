using System;

namespace OrderService.Application.Events
{
    /// <summary>
    /// Event representing an updated account
    /// </summary>
    public class AccountUpdated
    {
        /// <summary>
        /// Account ID
        /// </summary>
        public Guid AccountId { get; set; }
        
        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Full name
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        
        /// <summary>
        /// Phone number
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Date when the account was updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}