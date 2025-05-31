using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountService.Domain.Entities;
using AccountService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AccountService.Infrastructure.Data
{
    public class AccountContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        
        public AccountContext(DbContextOptions<AccountContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            // Account entity configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("accounts");
                
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(e => e.Username)
                    .IsUnique();
                    
                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .IsRequired();
                    
                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.PhoneNumber)
                    .HasColumnName("phone_number")
                    .HasMaxLength(20);
                    
                entity.Property(e => e.Fullname)
                    .HasColumnName("fullname")
                    .HasMaxLength(100);
                    
                entity.Property(e => e.AvatarURL)
                    .HasColumnName("avatar_url")
                    .HasMaxLength(255);
                    
                entity.Property(e => e.Role)
                    .HasColumnName("role")
                    .IsRequired()
                    .HasConversion<string>(); // Convert enum to string in database
                    
                entity.Property(e => e.RegistrationDate)
                    .HasColumnName("registration_date")
                    .IsRequired();
                    
                entity.Property(e => e.LastLoginDate)
                    .HasColumnName("last_login_date");
                    
                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .IsRequired();
                    
                entity.Property(e => e.IsVerified)
                    .HasColumnName("is_verified")
                    .IsRequired();
                    
                entity.Property(e => e.CompleteRate)
                    .HasColumnName("complete_rate")
                    .HasColumnType("decimal(5,2)");
                    
                entity.Property(e => e.ShopId)
                    .HasColumnName("shop_id");
                    
                // Base entity properties
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();
                    
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
                    .IsRequired();
                    
                // Query filter for soft delete
                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }
    }
}
