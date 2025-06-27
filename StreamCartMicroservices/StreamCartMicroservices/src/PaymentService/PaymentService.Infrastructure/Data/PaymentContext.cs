using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using ProductService.Domain.Enums;

namespace PaymentService.Infrastructure.Data
{
    public class PaymentContext : DbContext
    {
        public DbSet<Payment> Payments { get; set; } = null!;

        public PaymentContext(DbContextOptions<PaymentContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedNever();

                // Order relationship (for potential future integration)
                entity.Property(e => e.OrderId)
                    .IsRequired();
                entity.HasIndex(e => e.OrderId)
                    .HasDatabaseName("IX_Payments_OrderId");

                // User relationship
                entity.Property(e => e.UserId)
                    .IsRequired();
                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Payments_UserId");

                // Add index for payment status to improve query performance
                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("IX_Payments_Status");

                // Add index for QR code for fast lookups (changed from TransactionId)
                entity.HasIndex(e => e.QrCode)
                    .HasDatabaseName("IX_Payments_QrCode");


                // Money-related configurations
                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.Fee)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                // Enum configurations with conversions to string
                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.PaymentMethod)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                // String property configurations (changed from TransactionId to QrCode)
                entity.Property(e => e.QrCode)
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.LastModifiedBy)
                    .HasMaxLength(255);

                // Add timestamp for created and modified dates
                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.LastModifiedAt);

                // Flag for soft delete
                entity.Property(e => e.IsDeleted)
                    .HasDefaultValue(false);

                // Add index for processing date for date range queries
                entity.Property(e => e.ProcessedAt);
                entity.HasIndex(e => e.ProcessedAt)
                    .HasDatabaseName("IX_Payments_ProcessedAt");

                // Create combined indexes for common query patterns
                entity.HasIndex(e => new { e.UserId, e.Status })
                    .HasDatabaseName("IX_Payments_UserId_Status");

                entity.HasIndex(e => new { e.OrderId, e.Status })
                    .HasDatabaseName("IX_Payments_OrderId_Status");

                entity.HasIndex(e => new { e.CreatedAt, e.Status })
                    .HasDatabaseName("IX_Payments_CreatedAt_Status");

                // Add filter for all queries to exclude deleted items by default
                entity.HasQueryFilter(p => !p.IsDeleted);
            });
        }
    }
}