namespace UBB_SE_2026_923_2.Data;

using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Models;

/// <summary>
/// Single EF Core context for the entire application. Code-first; the schema
/// is created and evolved exclusively through EF Core migrations.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> databaseContextOptions)
        : base(databaseContextOptions)
    {
    }

    // -------- Core domain --------
    public DbSet<User> Users => this.Set<User>();

    public DbSet<Staff> StaffMembers => this.Set<Staff>();

    public DbSet<Doctor> Doctors => this.Set<Doctor>();

    public DbSet<Pharmacyst> Pharmacysts => this.Set<Pharmacyst>();

    // -------- Pharmacy --------
    public DbSet<Item> Items => this.Set<Item>();

    public DbSet<Substance> Substances => this.Set<Substance>();

    public DbSet<ItemSubstance> ItemSubstances => this.Set<ItemSubstance>();

    public DbSet<ItemBatch> ItemBatches => this.Set<ItemBatch>();

    public DbSet<Order> Orders => this.Set<Order>();

    public DbSet<OrderItem> OrderItems => this.Set<OrderItem>();

    public DbSet<UserDiscount> UserDiscounts => this.Set<UserDiscount>();

    public DbSet<UserNotification> UserNotifications => this.Set<UserNotification>();

    public DbSet<PeriodNote> PeriodNotes => this.Set<PeriodNote>();

    // -------- Hospital --------
    public DbSet<Shift> Shifts => this.Set<Shift>();

    public DbSet<ShiftSwapRequest> ShiftSwapRequests => this.Set<ShiftSwapRequest>();

    public DbSet<Appointment> Appointments => this.Set<Appointment>();

    public DbSet<MedicalEvaluation> MedicalEvaluations => this.Set<MedicalEvaluation>();

    public DbSet<ERRequest> ERRequests => this.Set<ERRequest>();

    public DbSet<Notification> Notifications => this.Set<Notification>();

    public DbSet<Hangout> Hangouts => this.Set<Hangout>();

    public DbSet<HangoutParticipant> HangoutParticipants => this.Set<HangoutParticipant>();

    public DbSet<PharmacyHandover> PharmacyHandovers => this.Set<PharmacyHandover>();

    public DbSet<HighRiskMedicine> HighRiskMedicines => this.Set<HighRiskMedicine>();

    protected override void OnModelCreating(ModelBuilder databaseModelBuilder)
    {
        base.OnModelCreating(databaseModelBuilder);

        ConfigureStaffHierarchy(databaseModelBuilder);
        ConfigureUserAndPharmacy(databaseModelBuilder);
        ConfigureHospital(databaseModelBuilder);
        ConfigureReferenceData(databaseModelBuilder);
    }

    private static void ConfigureStaffHierarchy(ModelBuilder databaseModelBuilder)
    {
        // TPH: Staff is the base table; Doctor and Pharmacyst share it.
        // The existing Role string property doubles as the discriminator.
        databaseModelBuilder.Entity<Staff>(staffEntityBuilder =>
        {
            staffEntityBuilder.ToTable("Staff");
            staffEntityBuilder.HasKey(staff => staff.StaffID);
            staffEntityBuilder.Property(staff => staff.StaffID).ValueGeneratedOnAdd();

            staffEntityBuilder.Property(staff => staff.Email).HasMaxLength(256);
            staffEntityBuilder.Property(staff => staff.PasswordHash).HasMaxLength(512);
            staffEntityBuilder.Property(staff => staff.Role).HasMaxLength(50).IsRequired();
            staffEntityBuilder.Property(staff => staff.Department).HasMaxLength(100);
            staffEntityBuilder.Property(staff => staff.FirstName).HasMaxLength(100);
            staffEntityBuilder.Property(staff => staff.LastName).HasMaxLength(100);
            staffEntityBuilder.Property(staff => staff.ContactInfo).HasMaxLength(200);
            staffEntityBuilder.Property(staff => staff.LicenseNumber).HasMaxLength(100);
            staffEntityBuilder.Property(staff => staff.Specialization).HasMaxLength(100);
            staffEntityBuilder.Property(staff => staff.Status).HasMaxLength(50);
            staffEntityBuilder.Property(staff => staff.Certification).HasMaxLength(200);

            staffEntityBuilder.HasIndex(staff => staff.Email)
                  .IsUnique()
                  .HasFilter("[Email] IS NOT NULL AND [Email] <> ''");

            staffEntityBuilder.HasDiscriminator(staff => staff.Role)
                  .HasValue<Staff>("Staff")
                  .HasValue<Doctor>("Doctor")
                  .HasValue<Pharmacyst>("Pharmacist");
        });

        databaseModelBuilder.Entity<Doctor>(doctorEntityBuilder =>
        {
            doctorEntityBuilder.Property(doctor => doctor.DoctorStatus)
                  .HasConversion<string>()
                  .HasMaxLength(30);
        });
    }

    private static void ConfigureUserAndPharmacy(ModelBuilder databaseModelBuilder)
    {
        databaseModelBuilder.Entity<User>(userEntityBuilder =>
        {
            userEntityBuilder.ToTable("Users");
            userEntityBuilder.HasKey(user => user.Id);
            userEntityBuilder.Property(user => user.Id).ValueGeneratedOnAdd();
            userEntityBuilder.Property(user => user.Email).HasMaxLength(256).IsRequired();
            userEntityBuilder.Property(user => user.PhoneNumber).HasMaxLength(50).IsRequired();
            userEntityBuilder.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            userEntityBuilder.Property(user => user.Username).HasMaxLength(100).IsRequired();
            userEntityBuilder.Property(user => user.Role).HasMaxLength(20).HasDefaultValue("Client");
            userEntityBuilder.HasIndex(user => user.Email).IsUnique();

            userEntityBuilder.HasMany(user => user.Orders)
                  .WithOne(order => order.Client)
                  .HasForeignKey(order => order.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            userEntityBuilder.HasMany(user => user.PeriodNoteEntries)
                  .WithOne(periodNote => periodNote.User)
                  .HasForeignKey(periodNote => periodNote.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            userEntityBuilder.HasMany(user => user.UserDiscountEntries)
                  .WithOne(userDiscount => userDiscount.User)
                  .HasForeignKey(userDiscount => userDiscount.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            userEntityBuilder.HasMany(user => user.UserNotificationEntries)
                  .WithOne(userNotification => userNotification.User)
                  .HasForeignKey(userNotification => userNotification.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        databaseModelBuilder.Entity<Substance>(substanceEntityBuilder =>
        {
            substanceEntityBuilder.ToTable("Substances");
            substanceEntityBuilder.HasKey(substance => substance.Name);
            substanceEntityBuilder.Property(substance => substance.Name).HasMaxLength(255);
            substanceEntityBuilder.Property(substance => substance.Description).HasMaxLength(2000);

            substanceEntityBuilder.HasMany(substance => substance.ItemSubstanceEntries)
                  .WithOne(itemSubstanceLink => itemSubstanceLink.Substance)
                  .HasForeignKey(itemSubstanceLink => itemSubstanceLink.SubstanceName)
                  .OnDelete(DeleteBehavior.Cascade);

            // Reference data: curated list of pharmacological substances with
            // their lethal dose. Items reference these by Name, so the seed
            // values must exist before any Item -> ItemSubstance link can be
            // created. Item rows themselves stay user-generated.
            substanceEntityBuilder.HasData(
                new Substance { Name = "Ibuprofen", LethalDose = 3200.00f, Description = "Anti-inflammatory pain reliever" },
                new Substance { Name = "Paracetamol", LethalDose = 4000.00f, Description = "Pain reliever and fever reducer" },
                new Substance { Name = "Magnesium", LethalDose = 2500.00f, Description = "Mineral supplement for muscle and nerve support" },
                new Substance { Name = "Vitamin C", LethalDose = 2000.00f, Description = "Vitamin supplement for immune support" },
                new Substance { Name = "Cetirizine", LethalDose = 500.00f, Description = "Antihistamine for allergy relief" });
        });

        databaseModelBuilder.Entity<Item>(itemEntityBuilder =>
        {
            itemEntityBuilder.ToTable("Items");
            itemEntityBuilder.HasKey(item => item.Id);
            itemEntityBuilder.Property(item => item.Id).ValueGeneratedOnAdd();
            itemEntityBuilder.Property(item => item.Name).HasMaxLength(200).IsRequired();
            itemEntityBuilder.Property(item => item.Producer).HasMaxLength(200);
            itemEntityBuilder.Property(item => item.Category).HasMaxLength(100);
            itemEntityBuilder.Property(item => item.ImagePath).HasMaxLength(500);
            itemEntityBuilder.Property(item => item.Label).HasMaxLength(100);
            itemEntityBuilder.Property(item => item.Description).HasMaxLength(2000);

            itemEntityBuilder.HasMany(item => item.ItemSubstanceEntries)
                  .WithOne(itemSubstanceLink => itemSubstanceLink.Item)
                  .HasForeignKey(itemSubstanceLink => itemSubstanceLink.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            itemEntityBuilder.HasMany(item => item.ItemBatchEntries)
                  .WithOne(itemBatch => itemBatch.Item)
                  .HasForeignKey(itemBatch => itemBatch.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        databaseModelBuilder.Entity<ItemSubstance>(itemSubstanceEntityBuilder =>
        {
            itemSubstanceEntityBuilder.ToTable("ItemSubstances");
            itemSubstanceEntityBuilder.HasKey(itemSubstanceLink => new { itemSubstanceLink.ItemId, itemSubstanceLink.SubstanceName });
            itemSubstanceEntityBuilder.Property(itemSubstanceLink => itemSubstanceLink.SubstanceName).HasMaxLength(255);
        });

        databaseModelBuilder.Entity<ItemBatch>(itemBatchEntityBuilder =>
        {
            itemBatchEntityBuilder.ToTable("ItemBatches");
            itemBatchEntityBuilder.HasKey(itemBatch => new { itemBatch.ItemId, itemBatch.ExpirationDate });
        });

        databaseModelBuilder.Entity<Order>(orderEntityBuilder =>
        {
            orderEntityBuilder.ToTable("Orders");
            orderEntityBuilder.HasKey(order => order.Id);
            orderEntityBuilder.Property(order => order.Id).ValueGeneratedOnAdd();

            orderEntityBuilder.HasMany(order => order.OrderItemEntries)
                  .WithOne(orderItem => orderItem.Order)
                  .HasForeignKey(orderItem => orderItem.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        databaseModelBuilder.Entity<OrderItem>(orderItemEntityBuilder =>
        {
            orderItemEntityBuilder.ToTable("OrderItems");
            orderItemEntityBuilder.HasKey(orderItem => new { orderItem.OrderId, orderItem.ItemId });

            orderItemEntityBuilder.HasOne(orderItem => orderItem.Item)
                  .WithMany()
                  .HasForeignKey(orderItem => orderItem.ItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        databaseModelBuilder.Entity<UserDiscount>(userDiscountEntityBuilder =>
        {
            userDiscountEntityBuilder.ToTable("UserDiscounts");
            userDiscountEntityBuilder.HasKey(userDiscount => new { userDiscount.UserId, userDiscount.ItemId });

            userDiscountEntityBuilder.HasOne(userDiscount => userDiscount.Item)
                  .WithMany()
                  .HasForeignKey(userDiscount => userDiscount.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        databaseModelBuilder.Entity<UserNotification>(userNotificationEntityBuilder =>
        {
            userNotificationEntityBuilder.ToTable("UserNotifications");
            userNotificationEntityBuilder.HasKey(userNotification => new { userNotification.UserId, userNotification.ItemId });

            userNotificationEntityBuilder.HasOne(userNotification => userNotification.Item)
                  .WithMany()
                  .HasForeignKey(userNotification => userNotification.ItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        databaseModelBuilder.Entity<PeriodNote>(periodNoteEntityBuilder =>
        {
            periodNoteEntityBuilder.ToTable("PeriodNotes");
            periodNoteEntityBuilder.HasKey(periodNote => new { periodNote.UserId, periodNote.NoteId });
            periodNoteEntityBuilder.Property(periodNote => periodNote.NoteBody).HasMaxLength(2000);
        });
    }

    private static void ConfigureHospital(ModelBuilder databaseModelBuilder)
    {
        databaseModelBuilder.Entity<Shift>(shiftEntityBuilder =>
        {
            shiftEntityBuilder.ToTable("Shifts");
            shiftEntityBuilder.HasKey(shift => shift.Id);
            shiftEntityBuilder.Property(shift => shift.Id).ValueGeneratedOnAdd();
            shiftEntityBuilder.Property(shift => shift.Location).HasMaxLength(200);
            shiftEntityBuilder.Property(shift => shift.Status).HasConversion<string>().HasMaxLength(30);

            shiftEntityBuilder.HasOne(shift => shift.Staff)
                  .WithMany(staffMember => staffMember.Shifts)
                  .HasForeignKey(shift => shift.StaffId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        databaseModelBuilder.Entity<ShiftSwapRequest>(shiftSwapRequestEntityBuilder =>
        {
            shiftSwapRequestEntityBuilder.ToTable("ShiftSwapRequests");
            shiftSwapRequestEntityBuilder.HasKey(shiftSwapRequest => shiftSwapRequest.SwapId);
            shiftSwapRequestEntityBuilder.Property(shiftSwapRequest => shiftSwapRequest.SwapId).ValueGeneratedOnAdd();
            shiftSwapRequestEntityBuilder.Property(shiftSwapRequest => shiftSwapRequest.Status).HasConversion<string>().HasMaxLength(30);

            shiftSwapRequestEntityBuilder.HasOne(shiftSwapRequest => shiftSwapRequest.Shift)
                  .WithMany()
                  .HasForeignKey(shiftSwapRequest => shiftSwapRequest.ShiftId)
                  .OnDelete(DeleteBehavior.Restrict);

            shiftSwapRequestEntityBuilder.HasOne(shiftSwapRequest => shiftSwapRequest.Requester)
                  .WithMany(staffMember => staffMember.ShiftSwapRequestsAsRequester)
                  .HasForeignKey(shiftSwapRequest => shiftSwapRequest.RequesterId)
                  .OnDelete(DeleteBehavior.Restrict);

            shiftSwapRequestEntityBuilder.HasOne(shiftSwapRequest => shiftSwapRequest.Colleague)
                  .WithMany(staffMember => staffMember.ShiftSwapRequestsAsColleague)
                  .HasForeignKey(shiftSwapRequest => shiftSwapRequest.ColleagueId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        databaseModelBuilder.Entity<Appointment>(appointmentEntityBuilder =>
        {
            appointmentEntityBuilder.ToTable("Appointments");
            appointmentEntityBuilder.HasKey(appointment => appointment.Id);
            appointmentEntityBuilder.Property(appointment => appointment.Id).ValueGeneratedOnAdd();
            appointmentEntityBuilder.Property(appointment => appointment.PatientName).HasMaxLength(200);
            appointmentEntityBuilder.Property(appointment => appointment.DoctorName).HasMaxLength(200);
            appointmentEntityBuilder.Property(appointment => appointment.Status).HasMaxLength(50);
            appointmentEntityBuilder.Property(appointment => appointment.Type).HasMaxLength(100);
            appointmentEntityBuilder.Property(appointment => appointment.Location).HasMaxLength(200);
            appointmentEntityBuilder.Property(appointment => appointment.Notes).HasMaxLength(2000);

            appointmentEntityBuilder.HasOne(appointment => appointment.Doctor)
                  .WithMany(doctor => doctor.Appointments)
                  .HasForeignKey(appointment => appointment.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        databaseModelBuilder.Entity<MedicalEvaluation>(medicalEvaluationEntityBuilder =>
        {
            medicalEvaluationEntityBuilder.ToTable("MedicalEvaluations");
            medicalEvaluationEntityBuilder.HasKey(medicalEvaluation => medicalEvaluation.EvaluationID);
            medicalEvaluationEntityBuilder.Property(medicalEvaluation => medicalEvaluation.EvaluationID).ValueGeneratedOnAdd();
            medicalEvaluationEntityBuilder.Property(medicalEvaluation => medicalEvaluation.PatientId).HasMaxLength(100);
            medicalEvaluationEntityBuilder.Property(medicalEvaluation => medicalEvaluation.Symptoms).HasMaxLength(2000);
            medicalEvaluationEntityBuilder.Property(medicalEvaluation => medicalEvaluation.MedicationsList).HasMaxLength(2000);
            medicalEvaluationEntityBuilder.Property(medicalEvaluation => medicalEvaluation.Notes).HasMaxLength(2000);

            medicalEvaluationEntityBuilder.HasOne(medicalEvaluation => medicalEvaluation.Evaluator)
                  .WithMany(doctor => doctor.MedicalEvaluations)
                  .HasForeignKey(medicalEvaluation => medicalEvaluation.DoctorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        databaseModelBuilder.Entity<ERRequest>(emergencyRoomRequestEntityBuilder =>
        {
            emergencyRoomRequestEntityBuilder.ToTable("ERRequests");
            emergencyRoomRequestEntityBuilder.HasKey(emergencyRoomRequest => emergencyRoomRequest.Id);
            emergencyRoomRequestEntityBuilder.Property(emergencyRoomRequest => emergencyRoomRequest.Id).ValueGeneratedOnAdd();
            emergencyRoomRequestEntityBuilder.Property(emergencyRoomRequest => emergencyRoomRequest.Specialization).HasMaxLength(100);
            emergencyRoomRequestEntityBuilder.Property(emergencyRoomRequest => emergencyRoomRequest.Location).HasMaxLength(200);
            emergencyRoomRequestEntityBuilder.Property(emergencyRoomRequest => emergencyRoomRequest.Status).HasMaxLength(50);
            emergencyRoomRequestEntityBuilder.Property(emergencyRoomRequest => emergencyRoomRequest.AssignedDoctorName).HasMaxLength(200);

            emergencyRoomRequestEntityBuilder.HasOne(emergencyRoomRequest => emergencyRoomRequest.AssignedDoctor)
                  .WithMany()
                  .HasForeignKey(emergencyRoomRequest => emergencyRoomRequest.AssignedDoctorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        databaseModelBuilder.Entity<Notification>(notificationEntityBuilder =>
        {
            notificationEntityBuilder.ToTable("Notifications");
            notificationEntityBuilder.HasKey(notification => notification.Id);
            notificationEntityBuilder.Property(notification => notification.Id).ValueGeneratedOnAdd();
            notificationEntityBuilder.Property(notification => notification.Title).HasMaxLength(200);
            notificationEntityBuilder.Property(notification => notification.Message).HasMaxLength(2000);
            notificationEntityBuilder.Property(notification => notification.ActionButtonText).HasMaxLength(100);

            notificationEntityBuilder.HasOne(notification => notification.Recipient)
                  .WithMany(staffMember => staffMember.Notifications)
                  .HasForeignKey(notification => notification.RecipientStaffId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        databaseModelBuilder.Entity<Hangout>(hangoutEntityBuilder =>
        {
            hangoutEntityBuilder.ToTable("Hangouts");
            hangoutEntityBuilder.HasKey(hangout => hangout.HangoutID);
            hangoutEntityBuilder.Property(hangout => hangout.HangoutID).ValueGeneratedOnAdd();
            hangoutEntityBuilder.Property(hangout => hangout.Title).HasMaxLength(200);
            hangoutEntityBuilder.Property(hangout => hangout.Description).HasMaxLength(2000);

            hangoutEntityBuilder.HasMany(hangout => hangout.HangoutParticipantEntries)
                  .WithOne(hangoutParticipant => hangoutParticipant.Hangout)
                  .HasForeignKey(hangoutParticipant => hangoutParticipant.HangoutId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        databaseModelBuilder.Entity<HangoutParticipant>(hangoutParticipantEntityBuilder =>
        {
            hangoutParticipantEntityBuilder.ToTable("HangoutParticipants");
            hangoutParticipantEntityBuilder.HasKey(hangoutParticipant => new { hangoutParticipant.HangoutId, hangoutParticipant.StaffId });

            hangoutParticipantEntityBuilder.HasOne(hangoutParticipant => hangoutParticipant.Staff)
                  .WithMany(staffMember => staffMember.HangoutParticipantEntries)
                  .HasForeignKey(hangoutParticipant => hangoutParticipant.StaffId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        databaseModelBuilder.Entity<PharmacyHandover>(pharmacyHandoverEntityBuilder =>
        {
            pharmacyHandoverEntityBuilder.ToTable("PharmacyHandovers");
            pharmacyHandoverEntityBuilder.HasKey(pharmacyHandover => new { pharmacyHandover.PharmacistId, pharmacyHandover.HandoverDate });

            pharmacyHandoverEntityBuilder.HasOne(pharmacyHandover => pharmacyHandover.Pharmacist)
                  .WithMany()
                  .HasForeignKey(pharmacyHandover => pharmacyHandover.PharmacistId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReferenceData(ModelBuilder databaseModelBuilder)
    {
        databaseModelBuilder.Entity<HighRiskMedicine>(highRiskMedicineEntityBuilder =>
        {
            highRiskMedicineEntityBuilder.ToTable("HighRiskMedicines");
            highRiskMedicineEntityBuilder.HasKey(highRiskMedicine => highRiskMedicine.MedicineName);
            highRiskMedicineEntityBuilder.Property(highRiskMedicine => highRiskMedicine.MedicineName).HasMaxLength(200);
            highRiskMedicineEntityBuilder.Property(highRiskMedicine => highRiskMedicine.WarningMessage).HasMaxLength(1000);

            // Reference data consumed by MedicalEvaluationService.CheckMedicineConflict.
            highRiskMedicineEntityBuilder.HasData(
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