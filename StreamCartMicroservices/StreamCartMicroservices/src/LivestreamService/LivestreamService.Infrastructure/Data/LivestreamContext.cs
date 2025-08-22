using LivestreamService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace LivestreamService.Infrastructure.Data
{
    public class LivestreamDbContext : DbContext
    {
        public LivestreamDbContext(DbContextOptions<LivestreamDbContext> options) : base(options)
        {
        }

        public DbSet<Livestream> Livestreams { get; set; }
        public DbSet<LivestreamProduct> LivestreamProducts { get; set; }
        public DbSet<StreamEvent> StreamEvents { get; set; }
        public DbSet<StreamView> StreamViews { get; set; }

        public DbSet<LivestreamCart> LivestreamCarts { get; set; }
        public DbSet<LivestreamCartItem> LivestreamCartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var jsonOptions = new JsonSerializerOptions(); 
            var dictToJsonConverter = new ValueConverter<Dictionary<string, string>?, string?>(
                v => v == null || v.Count == 0 ? null : JsonSerializer.Serialize(v, jsonOptions),
                v => string.IsNullOrWhiteSpace(v) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonOptions)!
            );

            // Configure Livestream entity
            modelBuilder.Entity<Livestream>(entity =>
            {
                entity.ToTable("Livestreams");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.SellerId).IsRequired();
                entity.Property(e => e.ShopId).IsRequired();
                entity.Property(e => e.ScheduledStartTime).IsRequired();
                entity.Property(e => e.LivekitRoomId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StreamKey).HasMaxLength(100);
                entity.Property(e => e.PlaybackUrl).HasMaxLength(500);
                entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(500);

                // Add an index on Status to quickly find active livestreams
                entity.HasIndex(e => e.Status);

                // Add indexes on SellerId and ShopId
                entity.HasIndex(e => e.SellerId);
                entity.HasIndex(e => e.ShopId);

                // Add index on scheduled start time
                entity.HasIndex(e => e.ScheduledStartTime);
            });
            modelBuilder.Entity<LivestreamProduct>(entity =>
            {
                entity.ToTable("LivestreamProducts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LivestreamId).IsRequired();
                entity.Property(e => e.ProductId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.VariantId).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Stock).IsRequired();

                // Add indexes
                entity.HasIndex(e => e.LivestreamId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.IsPin);
            });
            modelBuilder.Entity<StreamEvent>(entity =>
            {
                entity.ToTable("StreamEvents");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LivestreamId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Payload).HasMaxLength(4000);

                entity.HasIndex(e => e.LivestreamId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.EventType);
            });
            modelBuilder.Entity<StreamView>(entity =>
            {
                entity.ToTable("StreamViews");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LivestreamId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();

                entity.HasIndex(e => e.LivestreamId);
                entity.HasIndex(e => e.UserId);
            });
            modelBuilder.Entity<LivestreamCart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LivestreamId).IsRequired();
                entity.Property(e => e.ViewerId).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Indexes
                entity.HasIndex(e => new { e.LivestreamId, e.ViewerId }).IsUnique();
                entity.HasIndex(e => e.ExpiresAt);

                // Relationships
                entity.HasOne(e => e.Livestream)
                      .WithMany()
                      .HasForeignKey(e => e.LivestreamId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // LivestreamCartItem configuration
            modelBuilder.Entity<LivestreamCartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.VariantId).HasMaxLength(50);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ShopName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PrimaryImage).IsRequired().HasMaxLength(500);
                entity.Property(e => e.LivestreamPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.OriginalPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Attributes)
                       .HasColumnType("jsonb")
                       .HasConversion(dictToJsonConverter);

                // Indexes
                entity.HasIndex(e => e.LivestreamCartId);
                entity.HasIndex(e => e.LivestreamProductId);
                entity.HasIndex(e => new { e.LivestreamCartId, e.LivestreamProductId, e.VariantId }).IsUnique();

                // Relationships
                entity.HasOne(e => e.LivestreamCart)
                      .WithMany(c => c.Items)
                      .HasForeignKey(e => e.LivestreamCartId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.LivestreamProduct)
              .WithMany()
              .HasForeignKey(e => e.LivestreamProductId)
              .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}