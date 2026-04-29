using Busly.API.Models;
using Microsoft.EntityFrameworkCore;
using BuslyRoute = Busly.API.Models.Route;

namespace Busly.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ────────────────────────────────────────────────────────────────
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<BusOperator> BusOperators => Set<BusOperator>();
    public DbSet<BuslyRoute> Routes => Set<BuslyRoute>();
    public DbSet<BusLayout> BusLayouts => Set<BusLayout>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<BusStop> BusStops => Set<BusStop>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookedSeat> BookedSeats => Set<BookedSeat>();
    public DbSet<SeatLock> SeatLocks { get; set; }
    public DbSet<BusOperatingDay> BusOperatingDays { get; set; }
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Cancellation> Cancellations => Set<Cancellation>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TcVersion> TcVersions => Set<TcVersion>();
    public DbSet<PlatformConfig> PlatformConfigs => Set<PlatformConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── admin ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Admin>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(a => a.Email).HasColumnName("email");
            e.HasIndex(a => a.Email).IsUnique();
        });

        // ── customer ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(c => c.Email).HasColumnName("email");
            e.HasIndex(c => c.Email).IsUnique();
            e.Property(c => c.TcAccepted).HasDefaultValue(false);
            e.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ── bus_operator ──────────────────────────────────────────────────────
        modelBuilder.Entity<BusOperator>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(o => o.Email).HasColumnName("email");
            e.HasIndex(o => o.Email).IsUnique();
            e.Property(o => o.TcAccepted).HasDefaultValue(false);
            e.Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.ToTable("bus_operator", t =>
                t.HasCheckConstraint(
                    "ck_bus_operator_status",
                    "status IN ('PENDING','APPROVED','DISABLED','REJECTED')"));

            e.HasOne(o => o.ApprovingAdmin)
             .WithMany(a => a.ApprovedOperators)
             .HasForeignKey(o => o.ApprovedByAdmin)
             .HasConstraintName("fk_bus_operator_admin")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── route ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<BuslyRoute>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(r => r.IsActive).HasDefaultValue(true);

            e.HasIndex(r => new { r.SourceCity, r.DestinationCity }).IsUnique();

            e.HasOne(r => r.Admin)
             .WithMany(a => a.CreatedRoutes)
             .HasForeignKey(r => r.CreatedByAdmin)
             .HasConstraintName("fk_route_admin")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── bus_layout ────────────────────────────────────────────────────────
        modelBuilder.Entity<BusLayout>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");

            // JSONB column
            e.Property(l => l.SeatConfig)
             .HasColumnName("seat_config")
             .HasColumnType("jsonb");
        });

        // ── bus ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<Bus>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(b => b.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.ToTable("bus", t =>
                t.HasCheckConstraint(
                    "ck_bus_status",
                    "status IN ('PENDING','ACTIVE','DISABLED','REMOVED','REJECTED')"));

            e.HasOne(b => b.Operator)
             .WithMany(o => o.Buses)
             .HasForeignKey(b => b.OperatorId)
             .HasConstraintName("fk_bus_operator")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.Route)
             .WithMany(r => r.Buses)
             .HasForeignKey(b => b.RouteId)
             .HasConstraintName("fk_bus_route")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.Layout)
             .WithMany(l => l.Buses)
             .HasForeignKey(b => b.LayoutId)
             .HasConstraintName("fk_bus_layout")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.ApprovingAdmin)
             .WithMany(a => a.ApprovedBuses)
             .HasForeignKey(b => b.ApprovedByAdmin)
             .HasConstraintName("fk_bus_admin")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── seat ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Seat>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");

            e.HasOne(s => s.Layout)
             .WithMany(l => l.Seats)
             .HasForeignKey(s => s.LayoutId)
             .HasConstraintName("fk_seat_layout")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── bus_stop ──────────────────────────────────────────────────────────
        modelBuilder.Entity<BusStop>(e =>
        {
            e.HasKey(bs => bs.Id);
            e.Property(bs => bs.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");

            e.ToTable("bus_stop", t =>
                t.HasCheckConstraint(
                    "ck_bus_stop_type",
                    "type IN ('BOARDING', 'DROPPING')"));

            e.HasOne(bs => bs.Bus)
             .WithMany(b => b.BusStops)
             .HasForeignKey(bs => bs.BusId)
             .HasConstraintName("fk_bus_stop_bus")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── booking ───────────────────────────────────────────────────────────
        // NOTE: coupon_id FK is configured here (deferred circular dependency).
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(b => b.BookedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // PNR: stored, indexed column — first 8 chars of booking ID (uppercase)
            e.Property(b => b.Pnr).HasColumnName("pnr").HasMaxLength(8);
            e.HasIndex(b => b.Pnr).IsUnique().HasDatabaseName("idx_booking_pnr");

            e.ToTable("booking", t =>
                t.HasCheckConstraint(
                    "ck_booking_status",
                    "status IN ('INITIATED','PAYMENT_PENDING','CONFIRMED','CANCELLED','REFUNDED')"));

            e.HasOne(b => b.Customer)
             .WithMany(c => c.Bookings)
             .HasForeignKey(b => b.CustomerId)
             .HasConstraintName("fk_booking_customer")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(b => b.Bus)
             .WithMany(bus => bus.Bookings)
             .HasForeignKey(b => b.BusId)
             .HasConstraintName("fk_booking_bus")
             .OnDelete(DeleteBehavior.SetNull);

            // Deferred FK: booking → coupon (circular dependency broken here)
            e.HasOne(b => b.Coupon)
             .WithMany(c => c.Bookings)
             .HasForeignKey(b => b.CouponId)
             .HasConstraintName("fk_booking_coupon")
             .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            e.HasIndex(b => b.CustomerId).HasDatabaseName("idx_booking_customer");
            e.HasIndex(b => b.BusId).HasDatabaseName("idx_booking_bus");
        });

        // ── cancellation ──────────────────────────────────────────────────────
        modelBuilder.Entity<Cancellation>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");

            e.ToTable("cancellation", t =>
            {
                t.HasCheckConstraint(
                    "ck_cancellation_cancelled_by",
                    "cancelled_by IN ('customer','operator')");
                t.HasCheckConstraint(
                    "ck_cancellation_refund_status",
                    "refund_status IN ('PENDING','PROCESSED','FAILED')");
            });

            e.HasOne(c => c.Booking)
             .WithMany(b => b.Cancellations)
             .HasForeignKey(c => c.BookingId)
             .HasConstraintName("fk_cancellation_booking")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── coupon ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Coupon>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(c => c.IsUsed).HasDefaultValue(false);

            e.HasIndex(c => c.Code).IsUnique();

            e.ToTable("coupon", t =>
                t.HasCheckConstraint(
                    "ck_coupon_discount_type",
                    "discount_type IN ('flat','percent')"));

            e.HasOne(c => c.Customer)
             .WithMany(cu => cu.IssuedCoupons)
             .HasForeignKey(c => c.IssuedToCustomer)
             .HasConstraintName("fk_coupon_customer")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(c => c.Cancellation)
             .WithOne(ca => ca.Coupon)
             .HasForeignKey<Coupon>(c => c.CancellationId)
             .HasConstraintName("fk_coupon_cancellation")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── booked_seat ───────────────────────────────────────────────────────
        modelBuilder.Entity<BookedSeat>(e =>
        {
            e.HasKey(bs => bs.Id);
            e.Property(bs => bs.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");

            // Unique index: prevents double booking at DB level
            e.HasIndex(bs => new { bs.SeatId, bs.BusId, bs.JourneyDate })
             .IsUnique()
             .HasDatabaseName("unique_seat_per_trip");

            e.HasOne(bs => bs.Booking)
             .WithMany(b => b.BookedSeats)
             .HasForeignKey(bs => bs.BookingId)
             .HasConstraintName("fk_booked_seat_booking")
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(bs => bs.Seat)
             .WithMany(s => s.BookedSeats)
             .HasForeignKey(bs => bs.SeatId)
             .HasConstraintName("fk_booked_seat_seat")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(bs => bs.Bus)
             .WithMany(b => b.BookedSeats)
             .HasForeignKey(bs => bs.BusId)
             .HasConstraintName("fk_booked_seat_bus")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── seat_lock ─────────────────────────────────────────────────────────
        modelBuilder.Entity<SeatLock>(e =>
        {
            e.HasKey(sl => sl.Id);
            e.Property(sl => sl.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(sl => sl.IsActive).HasDefaultValue(true);

            // Composite index for fast seat map load
            e.HasIndex(sl => new { sl.SeatId, sl.BusId, sl.JourneyDate, sl.IsActive })
             .HasDatabaseName("idx_seat_lock_lookup");

            e.HasOne(sl => sl.Seat)
             .WithMany(s => s.SeatLocks)
             .HasForeignKey(sl => sl.SeatId)
             .HasConstraintName("fk_seat_lock_seat")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(sl => sl.Customer)
             .WithMany(c => c.SeatLocks)
             .HasForeignKey(sl => sl.CustomerId)
             .HasConstraintName("fk_seat_lock_customer")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(sl => sl.Bus)
             .WithMany(b => b.SeatLocks)
             .HasForeignKey(sl => sl.BusId)
             .HasConstraintName("fk_seat_lock_bus")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── payment ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");

            e.ToTable("payment", t =>
                t.HasCheckConstraint(
                    "ck_payment_status",
                    "status IN ('PENDING','SUCCESS','FAILED','REFUNDED')"));

            e.HasOne(p => p.Booking)
             .WithMany(b => b.Payments)
             .HasForeignKey(p => p.BookingId)
             .HasConstraintName("fk_payment_booking")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── notification ──────────────────────────────────────────────────────
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");

            // XOR check: exactly one of customer_id / operator_id must be non-null
            e.ToTable("notification", t =>
                t.HasCheckConstraint(
                    "ck_notification_xor",
                    "(customer_id IS NOT NULL AND operator_id IS NULL) OR " +
                    "(customer_id IS NULL AND operator_id IS NOT NULL)"));

            e.HasOne(n => n.Customer)
             .WithMany(c => c.Notifications)
             .HasForeignKey(n => n.CustomerId)
             .HasConstraintName("fk_notification_customer")
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(n => n.Operator)
             .WithMany(o => o.Notifications)
             .HasForeignKey(n => n.OperatorId)
             .HasConstraintName("fk_notification_operator")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── audit_log ─────────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(a => a.PerformedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // JSONB column
            e.Property(a => a.Metadata)
             .HasColumnName("metadata")
             .HasColumnType("jsonb");

            e.ToTable("audit_log", t =>
                t.HasCheckConstraint(
                    "ck_audit_log_actor_role",
                    "actor_role IN ('admin','customer','operator')"));
        });

        // ── tc_version ────────────────────────────────────────────────────────
        modelBuilder.Entity<TcVersion>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(t => t.IsActive).HasDefaultValue(false);
            e.Property(t => t.PublishedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            e.HasIndex(t => t.Version).IsUnique();

            e.HasOne(t => t.Admin)
             .WithMany(a => a.PublishedTcVersions)
             .HasForeignKey(t => t.PublishedByAdmin)
             .HasConstraintName("fk_tc_version_admin")
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── bus_operating_days ─────────────────────────────────────────────────
        modelBuilder.Entity<BusOperatingDay>(e =>
        {
            e.HasKey(bod => bod.Id);
            e.Property(bod => bod.Id).HasColumnName("id").HasDefaultValueSql("uuid_generate_v4()");
            e.Property(bod => bod.BusId).HasColumnName("bus_id");
            e.Property(bod => bod.DayOfWeek).HasColumnName("day_of_week");
            e.Property(bod => bod.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            e.Property(bod => bod.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            e.Property(bod => bod.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Unique constraint on bus_id and day_of_week
            e.HasIndex(bod => new { bod.BusId, bod.DayOfWeek }).IsUnique();

            e.HasOne(bod => bod.Bus)
             .WithMany(b => b.OperatingDays)
             .HasForeignKey(bod => bod.BusId)
             .HasConstraintName("fk_bus_operating_days_bus")
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── platform_config ───────────────────────────────────────────────────
        modelBuilder.Entity<PlatformConfig>(e =>
        {
            e.HasKey(p => p.Key);
        });
    }
}
