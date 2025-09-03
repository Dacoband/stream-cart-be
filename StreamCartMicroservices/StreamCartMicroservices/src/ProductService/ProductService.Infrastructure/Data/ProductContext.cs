using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System.Reflection;

namespace ProductService.Infrastructure.Data
{
    public class ProductContext : DbContext
    {
        // Add all required DbSet properties
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }
        public DbSet<ProductCombination> ProductCombinations { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FlashSale> FlashSale { get; set; }


        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configure Product entity for PostgreSQL
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.RatingAverage).HasColumnName("rating_average");

                entity.Property(e => e.ProductName)
                    .HasColumnName("product_name")
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasMaxLength(2000);

                entity.Property(e => e.SKU)
                    .HasColumnName("sku")
                    .HasMaxLength(50);

                entity.Property(e => e.CategoryId)
                    .HasColumnName("category_id");

                entity.Property(e => e.BasePrice)
                    .HasColumnName("base_price")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.DiscountPrice)
                    .HasColumnName("discount_price")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.StockQuantity)
                    .HasColumnName("stock_quantity");

                entity.Property(e => e.ReserveStock)
                   .HasColumnName("reserve_stock");

                entity.Property(e => e.Weight)
                    .HasColumnName("weight")
                    .HasColumnType("decimal(10,2)");
                entity.Property(e => e.Length)
                    .HasColumnName("length")
                    .HasMaxLength(100);
                entity.Property(e => e.Width)
                    .HasColumnName("width")
                    .HasMaxLength(100);
                entity.Property(e => e.Height)
                    .HasColumnName("height")
                    .HasMaxLength(100);

                entity.Property(e => e.HasVariant)
                    .HasColumnName("has_variant");

                entity.Property(e => e.ShopId)
                    .HasColumnName("shop_id");
                entity.Property(p => p.StartTime);

                entity.Property(p => p.EndTime);

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active");

                entity.Property(e => e.QuantitySold)
                    .HasColumnName("quantity_sold");

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
                entity.HasIndex(e => e.SKU)
                    .HasDatabaseName("ix_products_sku");

                entity.HasIndex(e => e.CategoryId)
                    .HasDatabaseName("ix_products_category_id");

                entity.HasIndex(e => e.ShopId)
                    .HasDatabaseName("ix_products_shop_id");

                entity.HasIndex(e => e.QuantitySold)
                    .HasDatabaseName("ix_products_quantity_sold");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("ix_products_is_active");

                // Soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Inside OnModelCreating method
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("ProductVariants");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.SKU)
                    .HasMaxLength(50);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .IsRequired();

                entity.Property(e => e.FlashSalePrice)
                    .HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Stock)
                    .IsRequired();

                entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Base entity properties
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.LastModifiedAt);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(50);
                entity.Property(e => e.IsDeleted);
                entity.Property(e => e.Weight)
                   .HasColumnName("weight")
                   .HasColumnType("decimal(10,2)");
                entity.Property(e => e.Length)
                    .HasColumnName("length")
                    .HasMaxLength(100);
                entity.Property(e => e.Width)
                    .HasColumnName("width")
                    .HasMaxLength(100);
                entity.Property(e => e.Height)
                    .HasColumnName("height")
                    .HasMaxLength(100);
                // Add soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<ProductAttribute>(entity =>
            {
                entity.ToTable("ProductAttributes");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(e => e.Name).IsUnique();

                // Base entity properties
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.LastModifiedAt);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(50);
                entity.Property(e => e.IsDeleted);

                // Add soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<AttributeValue>(entity =>
            {
                entity.ToTable("AttributeValues");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ValueName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasOne<ProductAttribute>()
                    .WithMany()
                    .HasForeignKey(e => e.AttributeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.AttributeId, e.ValueName }).IsUnique();

                // Base entity properties
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.LastModifiedAt);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(50);
                entity.Property(e => e.IsDeleted);

                // Add soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<ProductCombination>(entity =>
            {
                entity.ToTable("ProductCombinations");

                entity.HasKey(e => new { e.VariantId, e.AttributeValueId });

                entity.HasOne<ProductVariant>()
                    .WithMany()
                    .HasForeignKey(e => e.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<AttributeValue>()
                    .WithMany()
                    .HasForeignKey(e => e.AttributeValueId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Base entity properties
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.LastModifiedAt);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(50);
                entity.Property(e => e.IsDeleted);

                // Add soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Configure ProductImage entity
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ImageUrl).IsRequired();
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.IsPrimary).HasDefaultValue(false);
                entity.Property(e => e.AltText).HasMaxLength(200);

                // Add relationship with Product
                entity.HasOne<Product>()
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Add relationship with ProductVariant if applicable
                entity.HasOne<ProductVariant>()
                      .WithMany()
                      .HasForeignKey(e => e.VariantId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // Base entity properties
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.LastModifiedAt);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(50);
                entity.Property(e => e.IsDeleted);

                // Add soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);

                // Add index for faster queries
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.VariantId);
                entity.HasIndex(e => e.IsPrimary);
            });
            modelBuilder.Entity<Category>()
                .ToTable("Category")
                .HasKey(c => c.Id); // Dùng Id từ BaseEntity

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>()
                .Property(c => c.CategoryName)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<FlashSale>(entity =>
            {
                entity.ToTable("Flash-Sales");
                entity.HasKey(e => e.Id);

                // Configure Slot column with constraints
                entity.Property(e => e.Slot)
                      .HasColumnName("Slot")
                      .IsRequired()
                      .HasDefaultValue(1);

                // Add check constraint for Slot range (1-8)
                entity.HasCheckConstraint("CK_FlashSales_Slot", "\"Slot\" >= 1 AND \"Slot\" <= 8");

                // Configure other properties
                entity.Property(e => e.ProductId)
                      .HasColumnName("ProductID")
                      .IsRequired();

                entity.Property(e => e.VariantId)
                      .HasColumnName("VariantID");

                entity.Property(e => e.FlashSalePrice)
                      .HasColumnName("FlashSalePrice")
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                entity.Property(e => e.QuantityAvailable)
                      .HasColumnName("QuantityAvailable")
                      .IsRequired();

                entity.Property(e => e.QuantitySold)
                      .HasColumnName("QuantitySold")
                      .IsRequired();

                entity.Property(e => e.StartTime)
                      .HasColumnName("StartTime")
                      .IsRequired();

                entity.Property(e => e.EndTime)
                      .HasColumnName("EndTime")
                      .IsRequired();

                entity.Property(e => e.NotificationSent)
                      .HasDefaultValue(false);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.FlashSales)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_FlashSales_Products");

                // Indexes for performance
                entity.HasIndex(e => e.Slot)
                      .HasDatabaseName("IX_FlashSales_Slot");

                entity.HasIndex(e => new { e.Slot, e.StartTime, e.EndTime })
                      .HasDatabaseName("IX_FlashSales_Slot_StartTime_EndTime");

                entity.HasIndex(e => new { e.ProductId, e.VariantId, e.StartTime, e.EndTime })
                      .HasDatabaseName("IX_FlashSales_Product_Variant_Time");

                entity.HasIndex(e => new { e.StartTime, e.EndTime })
                      .HasDatabaseName("IX_FlashSales_TimeRange");
            });

            base.OnModelCreating(modelBuilder);

        }
    }
}