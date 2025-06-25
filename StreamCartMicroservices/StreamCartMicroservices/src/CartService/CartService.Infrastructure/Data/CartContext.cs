using CartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Domain.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Infrastructure.Data
{
    public class CartContext : DbContext
    {
        public CartContext(DbContextOptions<CartContext> options) : base(options) { }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Carts table
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Carts");

                entity.HasKey(c => c.Id);

                entity.HasMany(c => c.Items)
                      .WithOne(i => i.Cart)
                      .HasForeignKey(i => i.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItems table
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");

                entity.HasKey(i => i.Id);

                entity.Property(i => i.ProductId).IsRequired();
                entity.Property(i => i.VariantId).IsRequired(false);
                entity.Property(i => i.ShopId).IsRequired();

                entity.Property(i => i.ProductName).HasMaxLength(255).IsRequired();
                entity.Property(i => i.ShopName).HasMaxLength(255).IsRequired();
                entity.Property(i => i.PriceSnapShot).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(i => i.PriceCurrent).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(i => i.Quantity).IsRequired();
                entity.Property(i => i.Stock).IsRequired();

                entity.Property(i => i.PrimaryImage).HasMaxLength(500).IsRequired();
                entity.Property(i => i.Attributes).HasColumnType("jsonb");
            });
        }

    }
}
