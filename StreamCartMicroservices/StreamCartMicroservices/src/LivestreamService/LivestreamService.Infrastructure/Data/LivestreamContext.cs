using LivestreamService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }
    }
}