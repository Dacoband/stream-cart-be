using Appwrite.Services;
using Microsoft.EntityFrameworkCore;
using ShopService.Domain.Entities;
using System.Reflection;

namespace ShopService.Infrastructure.Data
{
    public class ShopContext : DbContext
    {
        // Add all required DbSet properties
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<ShopVoucher> ShopVouchers { get; set; }


        public ShopContext(DbContextOptions<ShopContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configure Shop entity for PostgreSQL
            modelBuilder.Entity<Shop>(entity =>
            {
                entity.ToTable("shops");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.ShopName)
                    .HasColumnName("shop_name")
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(2000);

                entity.Property(e => e.LogoURL)
                    .HasColumnName("logo_url")
                    .HasMaxLength(500);

                entity.Property(e => e.CoverImageURL)
                    .HasColumnName("cover_image_url")
                    .HasMaxLength(500);

                entity.Property(e => e.RatingAverage)
                    .HasColumnName("rating_average")
                    .HasColumnType("decimal(3,2)");

                entity.Property(e => e.TotalReview)
                    .HasColumnName("total_review");

                entity.Property(e => e.RegistrationDate)
                    .HasColumnName("registration_date");

                entity.Property(e => e.ApprovalStatus)
                    .HasColumnName("approval_status")
                    .HasConversion<string>();

                entity.Property(e => e.ApprovalDate)
                    .HasColumnName("approval_date");

                entity.Property(e => e.BankAccountNumber)
                    .HasColumnName("bank_account_number")
                    .HasMaxLength(50);

                entity.Property(e => e.BankName)
                    .HasColumnName("bank_name")
                    .HasMaxLength(200);

                entity.Property(e => e.TaxNumber)
                    .HasColumnName("tax_number")
                    .HasMaxLength(50);

                entity.Property(e => e.TotalProduct)
                    .HasColumnName("total_product");

                entity.Property(e => e.CompleteRate)
                    .HasColumnName("complete_rate")
                    .HasColumnType("decimal(5,2)");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasConversion<string>();
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
                entity.HasIndex(e => e.ShopName)
                    .HasDatabaseName("ix_shops_shop_name");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("ix_shops_status");

                entity.HasIndex(e => e.ApprovalStatus)
                    .HasDatabaseName("ix_shops_approval_status");

                entity.HasIndex(e => e.RatingAverage)
                    .HasDatabaseName("ix_shops_rating_average");

                // Soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("wallets");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.OwnerType)
                    .HasColumnName("owner_type")
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Balance)
                    .HasColumnName("balance")
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at");

                entity.Property(e => e.BankName)
                    .HasColumnName("bank_name")
                    .HasMaxLength(200);

                entity.Property(e => e.BankAccountNumber)
                    .HasColumnName("bank_account_number")
                    .HasMaxLength(50);

                entity.Property(e => e.ShopId)
                    .HasColumnName("shop_id");
                
                entity.HasOne<Shop>()
                    .WithOne() 
                    .HasForeignKey<Wallet>(w => w.ShopId)
                    .HasConstraintName("fk_wallets_shops")
                    .OnDelete(DeleteBehavior.Cascade); 

                
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

                
                entity.HasIndex(e => e.ShopId)
                    .HasDatabaseName("ix_wallets_shop_id")
                    .IsUnique(); // Ensure each shop has only one wallet

                // Soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });


            modelBuilder.Entity<ShopVoucher>(entity =>
            {
                entity.ToTable("shop_vouchers");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.ShopId)
                    .HasColumnName("shop_id")
                    .IsRequired();

                entity.Property(e => e.Code)
                    .HasColumnName("code")
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(500);

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(e => e.Value)
                    .HasColumnName("value")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.MaxValue)
                    .HasColumnName("max_value")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.MinOrderAmount)
                    .HasColumnName("min_order_amount")
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.StartDate)
                    .HasColumnName("start_date")
                    .IsRequired();

                entity.Property(e => e.EndDate)
                    .HasColumnName("end_date")
                    .IsRequired();

                entity.Property(e => e.AvailableQuantity)
                    .HasColumnName("available_quantity")
                    .IsRequired();

                entity.Property(e => e.UsedQuantity)
                    .HasColumnName("used_quantity")
                    .HasDefaultValue(0);

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasDefaultValue(true);

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
                    .HasColumnName("is_deleted")
                    .HasDefaultValue(false);

                // Foreign key relationship
                entity.HasOne(v => v.Shop)
                    .WithMany()
                    .HasForeignKey(v => v.ShopId)
                    .HasConstraintName("fk_shop_vouchers_shops")
                    .OnDelete(DeleteBehavior.Cascade);
                // Indexes for PostgreSQL
                entity.HasIndex(e => e.Code)
                    .HasDatabaseName("ix_shop_vouchers_code")
                    .IsUnique();

                entity.HasIndex(e => e.ShopId)
                    .HasDatabaseName("ix_shop_vouchers_shop_id");

                entity.HasIndex(e => e.Type)
                    .HasDatabaseName("ix_shop_vouchers_type");

                entity.HasIndex(e => e.StartDate)
                    .HasDatabaseName("ix_shop_vouchers_start_date");

                entity.HasIndex(e => e.EndDate)
                    .HasDatabaseName("ix_shop_vouchers_end_date");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("ix_shop_vouchers_is_active");

                // Composite index for active vouchers lookup
                entity.HasIndex(e => new { e.ShopId, e.IsActive, e.StartDate, e.EndDate })
                    .HasDatabaseName("ix_shop_vouchers_active_lookup");

                // Soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }
    }
}
