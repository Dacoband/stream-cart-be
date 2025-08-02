using System;
using System.Collections.Generic;
using Shared.Common.Domain.Bases;
using OrderService.Domain.Enums;
using System.Linq;
namespace OrderService.Domain.Entities
{
    public class Orders : BaseEntity
    {
        #region Properties    
        public string? VoucherCode { get; set; } = string.Empty;

        public string OrderCode { get; private set; }
        public DateTime OrderDate { get; private set; }
        public OrderStatus OrderStatus { get;  set; }
        public decimal TotalPrice { get;  set; }
        public decimal ShippingFee { get;  set; }

        public decimal DiscountAmount { get;  set; }
        public decimal FinalAmount { get;  set; }
        public decimal CommissionFee { get;  set; }
        public decimal NetAmount { get;  set; }
        public PaymentStatus PaymentStatus { get; private set; }
        public string CustomerNotes { get; private set; }
        public DateTime? EstimatedDeliveryDate { get;  set; }
        public DateTime? ActualDeliveryDate { get; private set; }
        public string TrackingCode { get; private set; }

        #region Shipping From Information
        public string FromAddress { get; private set; }
        public string FromWard { get; private set; }
        public string FromDistrict { get; private set; }
        public string FromProvince { get; private set; }
        public string FromPostalCode { get; private set; }
        public string FromShop { get; private set; }
        public string FromPhone { get; private set; }

        #endregion

        #region Shipping To Information
        public string ToAddress { get; private set; }
        public string ToWard { get; private set; }
        public string ToDistrict { get; private set; }
        public string ToProvince { get; private set; }
        public string ToPostalCode { get; private set; }
        public string ToName { get; private set; }
        public string ToPhone { get; private set; }

        #endregion

        #region Related IDs
        public Guid? LivestreamId { get; private set; }
        public Guid? CreatedFromCommentId { get; private set; }
        public Guid ShopId { get; private set; }
        public Guid AccountId { get; private set; }
        public Guid ShippingProviderId { get; private set; }

        #endregion
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
        private readonly List<OrderItem> _items = new List<OrderItem>();

        #endregion
        /// <summary>
        /// Confirms the order and updates to Pending status
        /// </summary>
        public void Confirm(string modifiedBy)
        {
            if (OrderStatus != OrderStatus.Waiting)
            {
                throw new InvalidOperationException($"Cannot confirm order with status {OrderStatus}");
            }

            OrderStatus = OrderStatus.Pending;
            SetModifier(modifiedBy);    
        }
        public void UpdateStatus(OrderStatus newStatus, string modifiedBy)
        {
            if (OrderStatus == OrderStatus.Completed || OrderStatus == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Không thể thay đổi trạng thái của đơn hàng đã hoàn tất hoặc đã hủy");
            }
            // Danh sách các trạng thái chuyển hợp lệ
            var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
    {
        { OrderStatus.Waiting, new() { OrderStatus.Pending, OrderStatus.Cancelled } },
        { OrderStatus.Pending, new() { OrderStatus.Processing, OrderStatus.Cancelled, OrderStatus.Waiting } },
        { OrderStatus.Processing, new() { OrderStatus.Packed } },
        { OrderStatus.Packed, new() { OrderStatus.OnDelivere } },
        { OrderStatus.Cancelled, new() { OrderStatus.Delivered } },
        { OrderStatus.Delivered, new() { OrderStatus.Completed, OrderStatus.Returning } },
        { OrderStatus.Returning, new() { OrderStatus.Refunded, OrderStatus.Completed } },
    };

            // Không được hủy nếu đã giao hàng
            if (newStatus == OrderStatus.Cancelled && OrderStatus == OrderStatus.Delivered)
            {
                throw new InvalidOperationException("Không thể hủy đơn hàng đã được giao");
            }

            // Kiểm tra trạng thái chuyển tiếp hợp lệ
            if (validTransitions.ContainsKey(OrderStatus))
            {
                var nextAllowed = validTransitions[OrderStatus];
                if (!nextAllowed.Contains(newStatus))
                {
                    throw new InvalidOperationException($"Không thể chuyển trạng thái từ {OrderStatus} sang {newStatus}");
                }
            }
            else if (newStatus != OrderStatus.Completed && newStatus != OrderStatus.Cancelled)
            {
                throw new InvalidOperationException($"Không thể chuyển trạng thái từ {OrderStatus} sang {newStatus}");
            }

            OrderStatus = newStatus;
            SetModifier(modifiedBy);
        }


        /// <summary>
        /// Auto-cancels an unconfirmed order due to shop timeout
        /// </summary>
        public void AutoCancel(string modifiedBy)
        {
            if (OrderStatus != OrderStatus.Waiting)
            {
                throw new InvalidOperationException($"Cannot auto-cancel order with status {OrderStatus}");
            }

            OrderStatus = OrderStatus.Cancelled;
            SetModifier(modifiedBy);
        }
        #region Constructors
        protected Orders()
        {
            OrderCode = string.Empty;
            CustomerNotes = string.Empty;
            TrackingCode = string.Empty;

            FromAddress = string.Empty;
            FromWard = string.Empty;
            FromDistrict = string.Empty;
            FromProvince = string.Empty;
            FromPostalCode = string.Empty;
            FromShop = string.Empty;
            FromPhone = string.Empty;

            ToAddress = string.Empty;
            ToWard = string.Empty;
            ToDistrict = string.Empty;
            ToProvince = string.Empty;
            ToPostalCode = string.Empty;
            ToName = string.Empty;
            ToPhone = string.Empty;
        }

        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="accountId">Account ID of the customer</param>
        /// <param name="shopId">Shop ID the order is placed with</param>
        /// <param name="toName">Recipient's name</param>
        /// <param name="toPhone">Recipient's phone</param>
        /// <param name="toAddress">Recipient's address</param>
        /// <param name="toWard">Recipient's ward</param>
        /// <param name="toDistrict">Recipient's district</param>
        /// <param name="toProvince">Recipient's province</param>
        /// <param name="toPostalCode">Recipient's postal code</param>
        /// <param name="fromAddress">Sender's address</param>
        /// <param name="fromWard">Sender's ward</param>
        /// <param name="fromDistrict">Sender's district</param>
        /// <param name="fromProvince">Sender's province</param>
        /// <param name="fromPostalCode">Sender's postal code</param>
        /// <param name="fromShop">Sender's shop name</param>
        /// <param name="fromPhone">Sender's phone</param>
        /// <param name="shippingProviderId">Shipping provider ID</param>
        /// <param name="customerNotes">Optional notes from customer</param>
        /// <param name="livestreamId">Optional livestream ID</param>
        /// <param name="createdFromCommentId">Optional comment ID</param>
        public Orders(
            Guid accountId,
            Guid shopId,
            string toName,
            string toPhone,
            string toAddress,
            string toWard,
            string toDistrict,
            string toProvince,
            string toPostalCode,
            string fromAddress,
            string fromWard,
            string fromDistrict,
            string fromProvince,
            string fromPostalCode,
            string fromShop,
            string fromPhone,
            Guid shippingProviderId,
            string customerNotes = "",
            Guid? livestreamId = null,
            Guid? createdFromCommentId = null
             )
        {
            if (accountId == Guid.Empty)
                throw new ArgumentException("Account ID cannot be empty", nameof(accountId));

            if (shopId == Guid.Empty)
                throw new ArgumentException("Shop ID cannot be empty", nameof(shopId));

            if (shippingProviderId == Guid.Empty)
                throw new ArgumentException("Shipping provider ID cannot be empty", nameof(shippingProviderId));

            // Generate a unique order code (format: ORD-{year}{month}{day}-{random 6 digits})
            OrderCode = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100000, 999999)}";
            OrderDate = DateTime.UtcNow;
            OrderStatus = OrderStatus.Waiting;
            PaymentStatus = PaymentStatus.pending;

            AccountId = accountId;
            ShopId = shopId;
            ShippingProviderId = shippingProviderId;
            
            // Shipping information
            ToName = toName ?? throw new ArgumentNullException(nameof(toName));
            ToPhone = toPhone ?? throw new ArgumentNullException(nameof(toPhone));
            ToAddress = toAddress ?? throw new ArgumentNullException(nameof(toAddress));
            ToWard = toWard ?? throw new ArgumentNullException(nameof(toWard));
            ToDistrict = toDistrict ?? throw new ArgumentNullException(nameof(toDistrict));
            ToProvince = toProvince ?? throw new ArgumentNullException(nameof(toProvince));
            ToPostalCode = toPostalCode ?? throw new ArgumentNullException(nameof(toPostalCode));
            
            FromAddress = fromAddress ?? throw new ArgumentNullException(nameof(fromAddress));
            FromWard = fromWard ?? throw new ArgumentNullException(nameof(fromWard));
            FromDistrict = fromDistrict ?? throw new ArgumentNullException(nameof(fromDistrict));
            FromProvince = fromProvince ?? throw new ArgumentNullException(nameof(fromProvince));
            FromPostalCode = fromPostalCode ?? throw new ArgumentNullException(nameof(fromPostalCode));
            FromShop = fromShop ?? throw new ArgumentNullException(nameof(fromShop));
            FromPhone = fromPhone ?? throw new ArgumentNullException(nameof(fromPhone));
            
            CustomerNotes = customerNotes ?? string.Empty;
            LivestreamId = livestreamId;
            CreatedFromCommentId = createdFromCommentId;
            
            TotalPrice = 0;
            ShippingFee = 0;
            DiscountAmount = 0;
            FinalAmount = 0;
            CommissionFee = 0;
            NetAmount = 0;
            TrackingCode = string.Empty;
        }

        #endregion

        #region Domain Methods

        /// <summary>
        /// Adds an item to the order and recalculates totals
        /// </summary>
        public void AddItem(OrderItem item)
        {
            if (OrderStatus != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Cannot add items to an order that is not pending");
            }
            
            _items.Add(item);
            RecalculateAmounts();
            SetModifier("System");
        }

        /// <summary>
        /// Adds multiple items to the order and recalculates totals
        /// </summary>
        public void AddItems(IEnumerable<OrderItem> items)
        {
            if (OrderStatus != OrderStatus.Waiting)
            {
                throw new InvalidOperationException("Cannot add items to an order that is not pending");
            }
            
            _items.AddRange(items);
            RecalculateAmounts();
            SetModifier("System");
        }

        /// <summary>
        /// Removes an item from the order and recalculates totals
        /// </summary>
        public void RemoveItem(Guid itemId)
        {
            if (OrderStatus != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Cannot remove items from an order that is not pending");
            }
            
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateAmounts();
                SetModifier("System");
            }
        }

        /// <summary>
        /// Sets the shipping fee and recalculates totals
        /// </summary>
        public void SetShippingFee(decimal fee, string modifiedBy)
        {
            if (fee < 0)
            {
                throw new ArgumentException("Shipping fee cannot be negative", nameof(fee));
            }
            
            ShippingFee = fee;
            RecalculateAmounts();
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Sets the discount amount and recalculates totals
        /// </summary>
        public void ApplyDiscount(decimal discount, string modifiedBy)
        {
            if (discount < 0)
            {
                throw new ArgumentException("Discount amount cannot be negative", nameof(discount));
            }
            
            if (discount > TotalPrice)
            {
                throw new ArgumentException("Discount amount cannot exceed total price", nameof(discount));
            }
            
            DiscountAmount = discount;
            RecalculateAmounts();
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Sets the estimated delivery date
        /// </summary>
        public void SetEstimatedDeliveryDate(DateTime date, string modifiedBy)
        {
            if (date < DateTime.UtcNow)
            {
                throw new ArgumentException("Estimated delivery date cannot be in the past", nameof(date));
            }
            
            EstimatedDeliveryDate = date;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Sets the tracking code for order
        /// </summary>
        public void SetTrackingCode(string trackingCode, string modifiedBy)
        {
            TrackingCode = trackingCode ?? throw new ArgumentNullException(nameof(trackingCode));
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Updates the order status to Processing and updates modifier
        /// </summary>
        public void Process(string modifiedBy)
        {
            if (OrderStatus != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Can only process orders in Pending status");
            }
            
            OrderStatus = OrderStatus.Processing;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Updates the payment status to Paid and updates modifier
        /// </summary>
        public void MarkAsPaid(string modifiedBy)
        {
            if (PaymentStatus != PaymentStatus.pending)
            {
                throw new InvalidOperationException("Can only mark pending payments as paid");
            }
            
            PaymentStatus = PaymentStatus.paid;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Updates the order status to Shipped and updates modifier
        /// </summary>
        public void Ship(string trackingCode, string modifiedBy)
        {
            if (OrderStatus != OrderStatus.Processing)
            {
                throw new InvalidOperationException("Can only ship orders in Processing status");
            }
            
            OrderStatus = OrderStatus.Shipped;
            TrackingCode = trackingCode ?? throw new ArgumentNullException(nameof(trackingCode));
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Updates the order status to Delivered and sets actual delivery date
        /// </summary>
        public void Deliver(string modifiedBy)
        {
            if (OrderStatus != OrderStatus.Shipped)
            {
                throw new InvalidOperationException("Can only deliver orders in Shipped status");
            }
            
            OrderStatus = OrderStatus.Delivered;
            ActualDeliveryDate = DateTime.UtcNow;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Updates the order status to Cancelled
        /// </summary>
        public void Cancel(string modifiedBy)
        {
            if (OrderStatus == OrderStatus.Delivered || OrderStatus == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot cancel orders that are already delivered or cancelled");
            }
            
            OrderStatus = OrderStatus.Cancelled;
            SetModifier(modifiedBy);
        }

        /// <summary>
        /// Recalculates all financial values for the order
        /// </summary>
        private void RecalculateAmounts()
        {
            TotalPrice = _items.Sum(item => item.TotalPrice);

            FinalAmount = TotalPrice + ShippingFee - DiscountAmount;
            CommissionFee = Math.Round(TotalPrice * 0.05m, 2);
            NetAmount = FinalAmount - CommissionFee;
        }

        #endregion
    }
}
