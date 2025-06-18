using System;

namespace ShopService.Application.DTOs
{
    public class ApprovalRequestDto
    {
        /// <summary>
        /// ID c?a ??i t??ng c?n ph� duy?t
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Lo?i ??i t??ng (Shop, Product, ...)
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// T�n c?a ??i t??ng
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Ng�y g?i y�u c?u
        /// </summary>
        public DateTime RequestDate { get; set; }
    }
}