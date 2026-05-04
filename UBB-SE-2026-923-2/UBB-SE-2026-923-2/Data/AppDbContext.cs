using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Staff> StaffMembers => Set<Staff>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftSwapRequest> ShiftSwapRequests => Set<ShiftSwapRequest>();
    public DbSet<MedicalEvaluation> MedicalEvaluations => Set<MedicalEvaluation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Hangout> Hangouts => Set<Hangout>();
    public DbSet<Substance> Substances => Set<Substance>();
    public DbSet<PharmacyHandover> PharmacyHandovers => Set<PharmacyHandover>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.PhoneNumber).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Username).IsRequired();
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(x => x.StaffID);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(x => x.StaffID);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<ShiftSwapRequest>(entity =>
        {
            entity.HasKey(x => x.SwapId);
        });

        modelBuilder.Entity<MedicalEvaluation>(entity =>
        {
            entity.HasKey(x => x.EvaluationID);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Hangout>(entity =>
        {
            entity.HasKey(x => x.HangoutID);
        });

        modelBuilder.Entity<Substance>(entity =>
        {
            entity.HasKey(x => x.Name);
        });

        modelBuilder.Entity<PharmacyHandover>(entity =>
        {
            entity.HasKey(x => new { x.PharmacistId, x.HandoverDate });
        });
    }
}
