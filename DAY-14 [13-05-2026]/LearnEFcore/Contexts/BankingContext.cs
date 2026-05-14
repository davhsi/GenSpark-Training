
using Microsoft.EntityFrameworkCore;
using LearnEFcore.Models;

namespace LearnEFcore.Contexts
{
    public class BankingContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=bankingdb;Username=postgres;Password=root");
        }

        public DbSet<Customer>  customers { get; set; }
        public DbSet<Account> accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(c =>
            {
                c.HasKey(c => c.Id);
                c.Property(c => c.DateOfBirth).HasColumnType("timestamp without time zone");
                //seeding
                c.HasData(new Customer() { Id = 101, Name = "Ramu", DateOfBirth = new DateTime(2000, 1, 1), Phone = "9876543210", Email = "ramu@gmail.com", Status = "Active" });
            });



            modelBuilder.Entity<Account>(a =>
            {
                a.HasKey(a => a.AccountNumber);

                a.HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .HasConstraintName("FK_Account_Customer")
                .OnDelete(DeleteBehavior.Restrict);

                a.Property(a => a.LastAccessed).HasColumnType("timestamp without time zone");

                a.HasData(new Account()
                {
                    AccountNumber = "0009998877",
                    Balance = 134.3M,
                    CustomerId = 101,
                    LastAccessed = new DateTime(2024, 5, 13, 11, 0, 0),
                    Status = "Active"
                });
            });

        }
    }
}