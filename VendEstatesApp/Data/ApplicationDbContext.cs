using Microsoft.EntityFrameworkCore;
using VendEstatesApp.Models;
using VendEstatesApp.Models.Enums;

namespace VendEstatesApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    public DbSet<Payroll> Payrolls => Set<Payroll>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    public DbSet<Contract> Contracts => Set<Contract>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<VehicleBooking> VehicleBookings => Set<VehicleBooking>();

    public DbSet<AgroInventory> AgroInventoryItems => Set<AgroInventory>();

    public DbSet<AgroSale> AgroSales => Set<AgroSale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureDecimalPrecision(modelBuilder);

        // Branch
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasIndex(b => b.Name).IsUnique();
        });

        // Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Employees)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Room
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(r => new { r.BranchId, r.RoomNumber }).IsUnique();

            entity.HasOne(r => r.Branch)
                .WithMany(b => b.Rooms)
                .HasForeignKey(r => r.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.CreatedByEmployee)
                .WithMany(e => e.BookingsCreated)
                .HasForeignKey(b => b.CreatedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(b => b.StayLengthInDays);
            entity.Ignore(b => b.IsLongTermStay);
            entity.Ignore(b => b.Balance);
        });

        // Contract
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasOne(c => c.Employee)
                .WithMany(e => e.Contracts)
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.VehicleBooking)
                .WithMany(v => v.Payments)
                .HasForeignKey(p => p.VehicleBookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Expense)
                .WithMany(e => e.Payments)
                .HasForeignKey(p => p.ExpenseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Payroll)
                .WithMany(pr => pr.Payments)
                .HasForeignKey(p => p.PayrollId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.ProcessedByEmployee)
                .WithMany()
                .HasForeignKey(p => p.ProcessedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Expense
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasOne(e => e.Branch)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RequestedByEmployee)
                .WithMany(emp => emp.RequestedExpenses)
                .HasForeignKey(e => e.RequestedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ApprovedByEmployee)
                .WithMany()
                .HasForeignKey(e => e.ApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveRequest
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasOne(l => l.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(l => l.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.ApprovedByEmployee)
                .WithMany()
                .HasForeignKey(l => l.ApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(l => l.NumberOfDays);
        });

        // Payroll
        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.HasIndex(p => new { p.EmployeeId, p.PayrollMonth, p.PayrollYear }).IsUnique();

            entity.HasOne(p => p.Employee)
                .WithMany(e => e.PayrollRecords)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.ApprovedByEmployee)
                .WithMany()
                .HasForeignKey(p => p.ApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.Employee)
                .WithMany(e => e.Notifications)
                .HasForeignKey(n => n.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PushSubscription
        modelBuilder.Entity<PushSubscription>(entity =>
        {
            entity.HasIndex(p => p.Endpoint).IsUnique();

            entity.HasOne(p => p.Employee)
                .WithMany(e => e.PushSubscriptions)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Vehicle
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasIndex(v => v.RegistrationNumber).IsUnique();

            entity.HasOne(v => v.Branch)
                .WithMany(b => b.Vehicles)
                .HasForeignKey(v => v.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // VehicleBooking
        modelBuilder.Entity<VehicleBooking>(entity =>
        {
            entity.HasOne(v => v.Vehicle)
                .WithMany(veh => veh.Bookings)
                .HasForeignKey(v => v.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.CreatedByEmployee)
                .WithMany()
                .HasForeignKey(v => v.CreatedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(v => v.RentalDays);
            entity.Ignore(v => v.Balance);
        });

        // AgroInventory
        modelBuilder.Entity<AgroInventory>(entity =>
        {
            entity.HasOne(a => a.Branch)
                .WithMany(b => b.AgroInventoryItems)
                .HasForeignKey(a => a.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(a => a.IsLowStock);
        });

        // AgroSale
        modelBuilder.Entity<AgroSale>(entity =>
        {
            entity.HasOne(a => a.AgroInventory)
                .WithMany(i => i.Sales)
                .HasForeignKey(a => a.AgroInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.SoldByEmployee)
                .WithMany()
                .HasForeignKey(a => a.SoldByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Store enums as strings for readability in the database.
        modelBuilder.Entity<Branch>().Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Employee>().Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Room>().Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Room>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Booking>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Payment>().Property(e => e.Type).HasConversion<string>().HasMaxLength(30);
        modelBuilder.Entity<Payment>().Property(e => e.Method).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Payment>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Expense>().Property(e => e.Category).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Expense>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<LeaveRequest>().Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<LeaveRequest>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Payroll>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Notification>().Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Contract>().Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Contract>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Vehicle>().Property(e => e.Category).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Vehicle>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<VehicleBooking>().Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<AgroInventory>().Property(e => e.Category).HasConversion<string>().HasMaxLength(20);
    }

    private static void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }
    }
}
