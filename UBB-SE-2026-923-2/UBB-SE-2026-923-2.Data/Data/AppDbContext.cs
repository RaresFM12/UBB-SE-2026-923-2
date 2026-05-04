using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.Data;

/// <summary>
/// Single EF Core context for the entire application. Code-first; the schema
/// is created and evolved exclusively through EF Core migrations.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // -------- Core domain --------
    public DbSet<User> Users => Set<User>();
    public DbSet<Staff> StaffMembers => Set<Staff>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Pharmacyst> Pharmacysts => Set<Pharmacyst>();

    // -------- Pharmacy --------
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Substance> Substances => Set<Substance>();
    public DbSet<ItemSubstance> ItemSubstances => Set<ItemSubstance>();
    public DbSet<ItemBatch> ItemBatches => Set<ItemBatch>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<UserDiscount> UserDiscounts => Set<UserDiscount>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<PeriodNote> PeriodNotes => Set<PeriodNote>();

    // -------- Hospital --------
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftSwapRequest> ShiftSwapRequests => Set<ShiftSwapRequest>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalEvaluation> MedicalEvaluations => Set<MedicalEvaluation>();
    public DbSet<ERRequest> ERRequests => Set<ERRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Hangout> Hangouts => Set<Hangout>();
    public DbSet<HangoutParticipant> HangoutParticipants => Set<HangoutParticipant>();
    public DbSet<PharmacyHandover> PharmacyHandovers => Set<PharmacyHandover>();
    public DbSet<HighRiskMedicine> HighRiskMedicines => Set<HighRiskMedicine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureStaffHierarchy(modelBuilder);
        ConfigureUserAndPharmacy(modelBuilder);
        ConfigureHospital(modelBuilder);
        ConfigureReferenceData(modelBuilder);
    }

    private static void ConfigureStaffHierarchy(ModelBuilder modelBuilder)
    {
        // TPH: Staff is the base table; Doctor and Pharmacyst share it.
        // The existing Role string property doubles as the discriminator.
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("Staff");
            entity.HasKey(s => s.StaffID);
            entity.Property(s => s.StaffID).ValueGeneratedOnAdd();

            entity.Property(s => s.Email).HasMaxLength(256);
            entity.Property(s => s.PasswordHash).HasMaxLength(512);
            entity.Property(s => s.Role).HasMaxLength(50).IsRequired();
            entity.Property(s => s.Department).HasMaxLength(100);
            entity.Property(s => s.FirstName).HasMaxLength(100);
            entity.Property(s => s.LastName).HasMaxLength(100);
            entity.Property(s => s.ContactInfo).HasMaxLength(200);
            entity.Property(s => s.LicenseNumber).HasMaxLength(100);
            entity.Property(s => s.Specialization).HasMaxLength(100);
            entity.Property(s => s.Status).HasMaxLength(50);
            entity.Property(s => s.Certification).HasMaxLength(200);

            entity.HasIndex(s => s.Email)
                  .IsUnique()
                  .HasFilter("[Email] IS NOT NULL AND [Email] <> ''");

            entity.HasDiscriminator(s => s.Role)
                  .HasValue<Staff>("Staff")
                  .HasValue<Doctor>("Doctor")
                  .HasValue<Pharmacyst>("Pharmacist");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.Property(d => d.DoctorStatus)
                  .HasConversion<string>()
                  .HasMaxLength(30);
        });
    }

    private static void ConfigureUserAndPharmacy(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PhoneNumber).HasMaxLength(50).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(u => u.Username).HasMaxLength(100).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasMany(u => u.Orders)
                  .WithOne(o => o.Client)
                  .HasForeignKey(o => o.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.PeriodNoteEntries)
                  .WithOne(p => p.User)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.UserDiscountEntries)
                  .WithOne(d => d.User)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.UserNotificationEntries)
                  .WithOne(n => n.User)
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Substance>(entity =>
        {
            entity.ToTable("Substances");
            entity.HasKey(s => s.Name);
            entity.Property(s => s.Name).HasMaxLength(255);
            entity.Property(s => s.Description).HasMaxLength(2000);

            entity.HasMany(s => s.ItemSubstanceEntries)
                  .WithOne(link => link.Substance)
                  .HasForeignKey(link => link.SubstanceName)
                  .OnDelete(DeleteBehavior.Cascade);

            // Reference data: curated list of pharmacological substances with
            // their lethal dose. Items reference these by Name, so the seed
            // values must exist before any Item -> ItemSubstance link can be
            // created. Item rows themselves stay user-generated.
            entity.HasData(
                new Substance { Name = "Ibuprofen",   LethalDose = 3200.00f, Description = "Anti-inflammatory pain reliever" },
                new Substance { Name = "Paracetamol", LethalDose = 4000.00f, Description = "Pain reliever and fever reducer" },
                new Substance { Name = "Magnesium",   LethalDose = 2500.00f, Description = "Mineral supplement for muscle and nerve support" },
                new Substance { Name = "Vitamin C",   LethalDose = 2000.00f, Description = "Vitamin supplement for immune support" },
                new Substance { Name = "Cetirizine",  LethalDose =  500.00f, Description = "Antihistamine for allergy relief" });
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Items");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).ValueGeneratedOnAdd();
            entity.Property(i => i.Name).HasMaxLength(200).IsRequired();
            entity.Property(i => i.Producer).HasMaxLength(200);
            entity.Property(i => i.Category).HasMaxLength(100);
            entity.Property(i => i.ImagePath).HasMaxLength(500);
            entity.Property(i => i.Label).HasMaxLength(100);
            entity.Property(i => i.Description).HasMaxLength(2000);

            entity.HasMany(i => i.ItemSubstanceEntries)
                  .WithOne(link => link.Item)
                  .HasForeignKey(link => link.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(i => i.ItemBatchEntries)
                  .WithOne(b => b.Item)
                  .HasForeignKey(b => b.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemSubstance>(entity =>
        {
            entity.ToTable("ItemSubstances");
            entity.HasKey(link => new { link.ItemId, link.SubstanceName });
            entity.Property(link => link.SubstanceName).HasMaxLength(255);
        });

        modelBuilder.Entity<ItemBatch>(entity =>
        {
            entity.ToTable("ItemBatches");
            entity.HasKey(b => new { b.ItemId, b.ExpirationDate });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).ValueGeneratedOnAdd();

            entity.HasMany(o => o.OrderItemEntries)
                  .WithOne(oi => oi.Order)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(oi => new { oi.OrderId, oi.ItemId });

            entity.HasOne(oi => oi.Item)
                  .WithMany()
                  .HasForeignKey(oi => oi.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserDiscount>(entity =>
        {
            entity.ToTable("UserDiscounts");
            entity.HasKey(d => new { d.UserId, d.ItemId });

            entity.HasOne(d => d.Item)
                  .WithMany()
                  .HasForeignKey(d => d.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("UserNotifications");
            entity.HasKey(n => new { n.UserId, n.ItemId });

            entity.HasOne(n => n.Item)
                  .WithMany()
                  .HasForeignKey(n => n.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PeriodNote>(entity =>
        {
            entity.ToTable("PeriodNotes");
            entity.HasKey(p => new { p.UserId, p.NoteId });
            entity.Property(p => p.NoteBody).HasMaxLength(2000);
        });
    }

    private static void ConfigureHospital(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shift>(entity =>
        {
            entity.ToTable("Shifts");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).ValueGeneratedOnAdd();
            entity.Property(s => s.Location).HasMaxLength(200);
            entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(s => s.Staff)
                  .WithMany(staff => staff.Shifts)
                  .HasForeignKey(s => s.StaffId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShiftSwapRequest>(entity =>
        {
            entity.ToTable("ShiftSwapRequests");
            entity.HasKey(s => s.SwapId);
            entity.Property(s => s.SwapId).ValueGeneratedOnAdd();
            entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(s => s.Shift)
                  .WithMany()
                  .HasForeignKey(s => s.ShiftId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Requester)
                  .WithMany(staff => staff.ShiftSwapRequestsAsRequester)
                  .HasForeignKey(s => s.RequesterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Colleague)
                  .WithMany(staff => staff.ShiftSwapRequestsAsColleague)
                  .HasForeignKey(s => s.ColleagueId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).ValueGeneratedOnAdd();
            entity.Property(a => a.PatientName).HasMaxLength(200);
            entity.Property(a => a.DoctorName).HasMaxLength(200);
            entity.Property(a => a.Status).HasMaxLength(50);
            entity.Property(a => a.Type).HasMaxLength(100);
            entity.Property(a => a.Location).HasMaxLength(200);
            entity.Property(a => a.Notes).HasMaxLength(2000);

            entity.HasOne(a => a.Doctor)
                  .WithMany(d => d.Appointments)
                  .HasForeignKey(a => a.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MedicalEvaluation>(entity =>
        {
            entity.ToTable("MedicalEvaluations");
            entity.HasKey(e => e.EvaluationID);
            entity.Property(e => e.EvaluationID).ValueGeneratedOnAdd();
            entity.Property(e => e.PatientId).HasMaxLength(100);
            entity.Property(e => e.Symptoms).HasMaxLength(2000);
            entity.Property(e => e.MedicationsList).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.Evaluator)
                  .WithMany(d => d.MedicalEvaluations)
                  .HasForeignKey(e => e.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ERRequest>(entity =>
        {
            entity.ToTable("ERRequests");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            entity.Property(r => r.Specialization).HasMaxLength(100);
            entity.Property(r => r.Location).HasMaxLength(200);
            entity.Property(r => r.Status).HasMaxLength(50);
            entity.Property(r => r.AssignedDoctorName).HasMaxLength(200);

            entity.HasOne(r => r.AssignedDoctor)
                  .WithMany()
                  .HasForeignKey(r => r.AssignedDoctorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Id).ValueGeneratedOnAdd();
            entity.Property(n => n.Title).HasMaxLength(200);
            entity.Property(n => n.Message).HasMaxLength(2000);
            entity.Property(n => n.ActionButtonText).HasMaxLength(100);

            entity.HasOne(n => n.Recipient)
                  .WithMany(s => s.Notifications)
                  .HasForeignKey(n => n.RecipientStaffId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Hangout>(entity =>
        {
            entity.ToTable("Hangouts");
            entity.HasKey(h => h.HangoutID);
            entity.Property(h => h.HangoutID).ValueGeneratedOnAdd();
            entity.Property(h => h.Title).HasMaxLength(200);
            entity.Property(h => h.Description).HasMaxLength(2000);

            entity.HasMany(h => h.HangoutParticipantEntries)
                  .WithOne(p => p.Hangout)
                  .HasForeignKey(p => p.HangoutId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HangoutParticipant>(entity =>
        {
            entity.ToTable("HangoutParticipants");
            entity.HasKey(p => new { p.HangoutId, p.StaffId });

            entity.HasOne(p => p.Staff)
                  .WithMany(s => s.HangoutParticipantEntries)
                  .HasForeignKey(p => p.StaffId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PharmacyHandover>(entity =>
        {
            entity.ToTable("PharmacyHandovers");
            entity.HasKey(h => new { h.PharmacistId, h.HandoverDate });

            entity.HasOne(h => h.Pharmacist)
                  .WithMany()
                  .HasForeignKey(h => h.PharmacistId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReferenceData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HighRiskMedicine>(entity =>
        {
            entity.ToTable("HighRiskMedicines");
            entity.HasKey(m => m.MedicineName);
            entity.Property(m => m.MedicineName).HasMaxLength(200);
            entity.Property(m => m.WarningMessage).HasMaxLength(1000);

            // Reference data consumed by MedicalEvaluationService.CheckMedicineConflict.
            entity.HasData(
                new HighRiskMedicine
                {
                    MedicineName = "Warfarin",
                    WarningMessage = "Anticoagulant - check INR before prescribing.",
                },
                new HighRiskMedicine
                {
                    MedicineName = "Methotrexate",
                    WarningMessage = "Hepatotoxic - confirm dosing and weekly schedule.",
                });
        });
    }
}
