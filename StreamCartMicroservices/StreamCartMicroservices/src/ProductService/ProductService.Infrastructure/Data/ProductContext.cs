using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System.Reflection;

namespace ProductService.Infrastructure.Data
{
    public class ProductContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

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

                entity.Property(e => e.Weight)
                    .HasColumnName("weight")
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Dimensions)
                    .HasColumnName("dimensions")
                    .HasMaxLength(100);

                entity.Property(e => e.HasVariant)
                    .HasColumnName("has_variant");

                entity.Property(e => e.ShopId)
                    .HasColumnName("shop_id");

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active");

                entity.Property(e => e.QuantitySold)
                    .HasColumnName("quantity_sold");

                entity.Property(e => e.LivestreamId)
                    .HasColumnName("livestream_id");

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

                entity.HasIndex(e => e.LivestreamId)
                    .HasDatabaseName("ix_products_livestream_id");

                // Soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }
    }
}