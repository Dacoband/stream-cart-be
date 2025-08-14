namespace ChatBoxService.Application.DTOs
{
    public class OrderIntentResult
    {
        public bool IsOrderIntent { get; set; }
        public OrderExtractedData? ExtractedData { get; set; }
        public float Confidence { get; set; }
        public string OrderType { get; set; } = string.Empty; // direct_sku, product_name, mixed
        public string OriginalMessage { get; set; } = string.Empty;
    }

    public class OrderExtractedData
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string? ProductName { get; set; }
    }

    public class OrderCreationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public OrderIntentResult? OrderIntent { get; set; }
        public LivestreamProductDTO? Product { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? StreamEventId { get; set; }
    }

    public class LivestreamProductDTO
    {
        public Guid Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string? VariantId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int ProductStock { get; set; }
        public string ProductImageUrl { get; set; } = string.Empty;
        public Guid ShopId { get; set; }
    }

    public class StreamEventResult
    {
        public Guid Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class UserAddressDTO
    {
        public Guid Id { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }

    public class ShopInfoDTO
    {
        public Guid Id { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}