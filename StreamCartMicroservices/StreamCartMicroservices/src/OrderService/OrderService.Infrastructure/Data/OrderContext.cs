using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Data
{
    /// <summary>
    /// Database context for Order service using PostgreSQL
    /// </summary>
    public class OrderContext : DbContext
    {
        /// <summary>
        /// Orders collection
        /// </summary>
        public DbSet<Orders> Orders { get; set; }

        /// <summary>
        /// Order items collection
        /// </summary>
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }

        /// <summary>
        /// Creates a new instance of OrderContext
        /// </summary>
        /// <param name="options">Database context options</param>
        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.HasPostgresEnum<OrderStatus>(); 




            // Configure Orders entity for PostgreSQL
            modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("orders");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.VoucherCode).HasColumnName("voucher_code");

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.OrderCode)
                    .HasColumnName("order_code")
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OrderDate)
                    .HasColumnName("order_date")
                    .IsRequired();

                entity.Property(e => e.OrderStatus)
               .HasColumnName("order_status")
               .IsRequired();


                entity.Property(e => e.TotalPrice)
                    .HasColumnName("total_price")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.ShippingFee)
                    .HasColumnName("shipping_fee")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DiscountAmount)
                    .HasColumnName("discount_amount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.FinalAmount)
                    .HasColumnName("final_amount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CommissionFee)
                    .HasColumnName("commission_fee")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.NetAmount)
                    .HasColumnName("net_amount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PaymentStatus)
                    .HasColumnName("payment_status");

                entity.Property(e => e.PaymentMethod)
                   .HasColumnName("payment_method")
                   .HasMaxLength(50);

                entity.Property(e => e.CustomerNotes)
                    .HasColumnName("customer_notes")
                    .HasMaxLength(1000);

                entity.Property(e => e.EstimatedDeliveryDate)
                    .HasColumnName("estimated_delivery_date");

                entity.Property(e => e.ActualDeliveryDate)
                    .HasColumnName("actual_delivery_date");

                entity.Property(e => e.TrackingCode)
                    .HasColumnName("tracking_code")
                    .HasMaxLength(100);

                entity.Property(e => e.TimeForShop)
                    .HasColumnName("time_for_shop");

                // Shipping From Information
                entity.Property(e => e.FromAddress)
                    .HasColumnName("from_address")
                    .HasMaxLength(500);

                entity.Property(e => e.FromWard)
                    .HasColumnName("from_ward")
                    .HasMaxLength(100);

                entity.Property(e => e.FromDistrict)
                    .HasColumnName("from_district")
                    .HasMaxLength(100);

                entity.Property(e => e.FromProvince)
                    .HasColumnName("from_province")
                    .HasMaxLength(100);

                entity.Property(e => e.FromPostalCode)
                    .HasColumnName("from_postal_code")
                    .HasMaxLength(20);

                entity.Property(e => e.FromShop)
                    .HasColumnName("from_shop")
                    .HasMaxLength(200);

                entity.Property(e => e.FromPhone)
                    .HasColumnName("from_phone")
                    .HasMaxLength(20);

                // Shipping To Information
                entity.Property(e => e.ToAddress)
                    .HasColumnName("to_address")
                    .HasMaxLength(500);

                entity.Property(e => e.ToWard)
                    .HasColumnName("to_ward")
                    .HasMaxLength(100);

                entity.Property(e => e.ToDistrict)
                    .HasColumnName("to_district")
                    .HasMaxLength(100);

                entity.Property(e => e.ToProvince)
                    .HasColumnName("to_province")
                    .HasMaxLength(100);

                entity.Property(e => e.ToPostalCode)
                    .HasColumnName("to_postal_code")
                    .HasMaxLength(20);

                entity.Property(e => e.ToName)
                    .HasColumnName("to_name")
                    .HasMaxLength(200);

                entity.Property(e => e.ToPhone)
                    .HasColumnName("to_phone")
                    .HasMaxLength(20);
                // Related IDs
                entity.Property(e => e.LivestreamId)
                    .HasColumnName("livestream_id");

                entity.Property(e => e.CreatedFromCommentId)
                    .HasColumnName("created_from_comment_id");

                entity.Property(e => e.ShopId)
                    .HasColumnName("shop_id")
                    .IsRequired();

                entity.Property(e => e.AccountId)
                    .HasColumnName("account_id")
                    .IsRequired();

                entity.Property(e => e.ShippingProviderId)
                    .HasColumnName("shipping_provider_id")
                    .IsRequired();

                // Base entity properties
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(50);

                entity.Property(e => e.LastModifiedAt)
                    .HasColumnName("last_modified_at");

                entity.Property(e => e.LastModifiedBy)
                    .HasColumnName("last_modified_by")
                    .HasMaxLength(50);

                entity.Property(e => e.IsDeleted)
                    .HasColumnName("is_deleted");

                // Indexes for PostgreSQL
                entity.HasIndex(e => e.OrderCode)
                    .HasDatabaseName("ix_orders_order_code")
                    .IsUnique();

                entity.HasIndex(e => e.OrderStatus)
                    .HasDatabaseName("ix_orders_order_status");

                entity.HasIndex(e => e.PaymentStatus)
                    .HasDatabaseName("ix_orders_payment_status");

                entity.HasIndex(e => e.AccountId)
                    .HasDatabaseName("ix_orders_account_id");

                entity.HasIndex(e => e.ShopId)
                    .HasDatabaseName("ix_orders_shop_id");

                entity.HasIndex(e => e.OrderDate)
                    .HasDatabaseName("ix_orders_order_date");

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure OrderItem entity for PostgreSQL
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.OrderId)
                    .HasColumnName("order_id")
                    .IsRequired();

                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                entity.Property(e => e.VariantId)
                    .HasColumnName("variant_id");

                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired();

                entity.Property(e => e.UnitPrice)
                    .HasColumnName("unit_price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.DiscountAmount)
                    .HasColumnName("discount_amount")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalPrice)
                    .HasColumnName("total_price")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Notes)
                    .HasColumnName("notes")
                    .HasMaxLength(500);

                entity.Property(e => e.RefundRequestId)
                    .HasColumnName("refund_request_id");

                // Base entity properties
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .HasMaxLength(50);

                entity.Property(e => e.LastModifiedAt)
                    .HasColumnName("last_modified_at");

                entity.Property(e => e.LastModifiedBy)
                    .HasColumnName("last_modified_by")
                    .HasMaxLength(50);

                entity.Property(e => e.IsDeleted)
                    .HasColumnName("is_deleted");

                // Relationships
                entity.HasOne<Orders>()
                    .WithMany(o => o.Items)
                    .HasForeignKey(oi => oi.OrderId)
                    .HasConstraintName("fk_order_items_orders")
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.OrderId)
                    .HasDatabaseName("ix_order_items_order_id");

                entity.HasIndex(e => e.ProductId)
                    .HasDatabaseName("ix_order_items_product_id");

                entity.HasQueryFilter(e => !e.IsDeleted);
            });
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ReviewText)
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(e => e.Rating)
                    .IsRequired();

                entity.Property(e => e.ImageUrls)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                        v => JsonSerializer.Deserialize<List<string>>(v, JsonSerializerOptions.Default) ?? new List<string>())
                    .HasColumnType("text");

                entity.HasIndex(e => e.ProductID);
                entity.HasIndex(e => e.OrderID);
                entity.HasIndex(e => e.LivestreamId);
                entity.HasIndex(e => e.AccountID);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Rating);
            });
            modelBuilder
                .HasPostgresEnum<PaymentStatus>();
        }
        private static class EnumConverters
        {
            public static OrderStatus ToOrderStatus(string? raw)
            {
                if (string.IsNullOrWhiteSpace(raw)) return OrderStatus.Waiting;
                var norm = raw.Trim();
                if (Enum.TryParse<OrderStatus>(norm, true, out var parsed))
                    return parsed;
                return OrderStatus.Waiting;
            }

            public static PaymentStatus ToPaymentStatus(string? raw)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    return PaymentStatus.Pending;

                if (Enum.TryParse<PaymentStatus>(raw.Trim(), true, out var parsed))
                    return parsed;

                return PaymentStatus.Pending;
            }
        }

    }
}

