using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Data;

public class LibraryContext : DbContext
{

    public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

    public DbSet<Member> Members { get; set; }
    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(b =>
        {
            b.HasKey(b => b.BookId).HasName("PK_BookId");
            b.Property(b => b.Title).IsRequired().HasMaxLength(200);
            b.Property(b => b.Author).IsRequired().HasMaxLength(200);
            b.Property(b => b.ISBN).HasMaxLength(20);
            b.Property(b => b.PublicationYear).IsRequired();
            b.Property(b => b.AvailableCopies).IsRequired();
            b.ToTable("Books");
        });


        modelBuilder.Entity<Member>(m =>
        {
            m.HasKey(m => m.MemberId).HasName("PK_MemberId");
            m.Property(m => m.FullName).IsRequired().HasMaxLength(100);
            m.Property(m => m.Email).IsRequired().HasMaxLength(100);
            m.Property(m => m.Phone).IsRequired().HasMaxLength(15);
            m.Property(m => m.JoinDate).IsRequired();
            m.ToTable("Members");
        });
    }
}

