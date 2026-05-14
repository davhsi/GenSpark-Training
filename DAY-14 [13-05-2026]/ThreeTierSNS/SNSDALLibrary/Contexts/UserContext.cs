using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SNSModelLibrary;

namespace SNSDALLibrary
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=notification_system_ado;Username=davishe");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                
                // Configure Owned Types
                entity.OwnsOne(u => u.Name, name =>
                {
                    name.Property(n => n.FirstName).HasColumnName("first_name");
                    name.Property(n => n.LastName).HasColumnName("last_name");
                });

                entity.OwnsOne(u => u.Phone, phone =>
                {
                    phone.Property(p => p.CountryCode).HasColumnName("country_code");
                    phone.Property(p => p.Number).HasColumnName("phone_number");
                });

                entity.Property(u => u.Email).IsRequired().HasColumnName("email");
                entity.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Message).IsRequired().HasColumnName("message");
                entity.Property(n => n.TimeStamp).HasColumnName("time_stamp");
                entity.Property(n => n.Type).HasColumnName("type");
                entity.Property(n => n.UserId).HasColumnName("user_id");

                // Relationship
                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
