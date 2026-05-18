using LibraryManagement.Model.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.DAL.Contexts
{
    public class LibraryContext : DbContext
{
    public DbSet<Member> Members { get; set; }
    public DbSet<MembershipType> MembershipTypes { get; set; }
    public DbSet<BookCategory> BookCategories { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookCopy> BookCopies { get; set; }
    public DbSet<Borrowing> Borrowings { get; set; }
    public DbSet<Fine> Fines { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=LibraryManagement;Username=postgres;Password=root");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(member =>
        {
            member.HasOne(m => m.MembershipType)
            .WithMany(mt => mt.Members)
            .HasForeignKey(m => m.MembershipTypeId)
            .OnDelete(DeleteBehavior.Restrict);

            // DB-level uniqueness for email and phone
            member.HasIndex(m => m.Email).IsUnique();
            member.HasIndex(m => m.Phone).IsUnique();
        });

        modelBuilder.Entity<Book>(book =>
        {
            book.HasOne(b => b.BookCategory)
            .WithMany(bc => bc.Books)
            .HasForeignKey(b => b.BookCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BookCopy>(bookcopy =>
        {
            bookcopy.HasOne(bc => bc.Book)
            .WithMany(b => b.BookCopies)
            .HasForeignKey(bc => bc.BookId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Borrowing>(borrowing =>
        {
            borrowing.HasOne(b => b.Member)
            .WithMany(m => m.Borrowings)
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

            borrowing.HasOne(b => b.BookCopy)
            .WithMany(bc => bc.Borrowings)
            .HasForeignKey(b => b.BookCopyId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Fine>(fine =>
        {
            fine.HasOne(f => f.Member)
            .WithMany(m => m.Fines)
            .HasForeignKey(f => f.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

            fine.HasOne(f => f.Borrowing)
            .WithOne(b => b.Fine)
            .HasForeignKey<Fine>(f => f.BorrowingId)
            .OnDelete(DeleteBehavior.Restrict);

        });

        // Seeding MembershipTypes
        modelBuilder.Entity<MembershipType>().HasData(
            new MembershipType { Id = 1, Name = "Basic",   MaxBorrowings = 2, MaxBorrowDays = 7  },
            new MembershipType { Id = 2, Name = "Student", MaxBorrowings = 3, MaxBorrowDays = 10 },
            new MembershipType { Id = 3, Name = "Premium", MaxBorrowings = 5, MaxBorrowDays = 15 }
        );

        // Seeding BookCategories
        modelBuilder.Entity<BookCategory>().HasData(
            new BookCategory { Id = 1, Name = "Fiction" },
            new BookCategory { Id = 2, Name = "Non-Fiction" },
            new BookCategory { Id = 3, Name = "Science" },
            new BookCategory { Id = 4, Name = "History" },
            new BookCategory { Id = 5, Name = "Biography" }
        );
    }
}
}
