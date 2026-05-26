namespace UBB_SE_2026_923_2.Migrations
{
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;
    using UBB_SE_2026_923_2.Data;

    [DbContext(typeof(AppDbContext))]
    [Migration("20260527121500_EnableDoctorSwapDemoUsers")]
    public partial class EnableDoctorSwapDemoUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE dbo.Users
                SET IsDisabled = 0, [Role] = N'Doctor'
                WHERE Email = N'house@hospital.local';

                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'cameron@hospital.local')
                    INSERT INTO dbo.Users (Email, PhoneNumber, PasswordHash, IsDisabled, IsAdmin, Username, [Role], DiscountNotifications, LoyaltyPoints, StartPeriodDate, CycleDays, PeriodLasts, PremenstrualSyndromeOption)
                    VALUES (N'cameron@hospital.local', N'0733333306', N'hashed_pwd_cameron', 0, 0, N'dr_cameron', N'Doctor', 0, 0, '1900-01-01', 28, 5, 0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'chase@hospital.local')
                    INSERT INTO dbo.Users (Email, PhoneNumber, PasswordHash, IsDisabled, IsAdmin, Username, [Role], DiscountNotifications, LoyaltyPoints, StartPeriodDate, CycleDays, PeriodLasts, PremenstrualSyndromeOption)
                    VALUES (N'chase@hospital.local', N'0733333307', N'hashed_pwd_chase', 0, 0, N'dr_chase', N'Doctor', 0, 0, '1900-01-01', 28, 5, 0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'foreman@hospital.local')
                    INSERT INTO dbo.Users (Email, PhoneNumber, PasswordHash, IsDisabled, IsAdmin, Username, [Role], DiscountNotifications, LoyaltyPoints, StartPeriodDate, CycleDays, PeriodLasts, PremenstrualSyndromeOption)
                    VALUES (N'foreman@hospital.local', N'0733333308', N'hashed_pwd_foreman', 0, 0, N'dr_foreman', N'Doctor', 0, 0, '1900-01-01', 28, 5, 0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'patel@hospital.local')
                    INSERT INTO dbo.Users (Email, PhoneNumber, PasswordHash, IsDisabled, IsAdmin, Username, [Role], DiscountNotifications, LoyaltyPoints, StartPeriodDate, CycleDays, PeriodLasts, PremenstrualSyndromeOption)
                    VALUES (N'patel@hospital.local', N'0733333309', N'hashed_pwd_patel', 0, 0, N'dr_patel', N'Doctor', 0, 0, '1900-01-01', 28, 5, 0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'rahman@hospital.local')
                    INSERT INTO dbo.Users (Email, PhoneNumber, PasswordHash, IsDisabled, IsAdmin, Username, [Role], DiscountNotifications, LoyaltyPoints, StartPeriodDate, CycleDays, PeriodLasts, PremenstrualSyndromeOption)
                    VALUES (N'rahman@hospital.local', N'0733333310', N'hashed_pwd_rahman', 0, 0, N'dr_rahman', N'Doctor', 0, 0, '1900-01-01', 28, 5, 0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'ionescu@hospital.local')
                    INSERT INTO dbo.Users (Email, PhoneNumber, PasswordHash, IsDisabled, IsAdmin, Username, [Role], DiscountNotifications, LoyaltyPoints, StartPeriodDate, CycleDays, PeriodLasts, PremenstrualSyndromeOption)
                    VALUES (N'ionescu@hospital.local', N'0733333311', N'hashed_pwd_ionescu', 0, 0, N'dr_ionescu', N'Doctor', 0, 0, '1900-01-01', 28, 5, 0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'popescu@hospital.local')
                    INSERT INTO dbo.Users (Email, PhoneNumber, PasswordHash, IsDisabled, IsAdmin, Username, [Role], DiscountNotifications, LoyaltyPoints, StartPeriodDate, CycleDays, PeriodLasts, PremenstrualSyndromeOption)
                    VALUES (N'popescu@hospital.local', N'0733333312', N'hashed_pwd_popescu', 0, 0, N'dr_popescu', N'Doctor', 0, 0, '1900-01-01', 28, 5, 0);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM dbo.Users
                WHERE Email IN (
                    N'cameron@hospital.local',
                    N'chase@hospital.local',
                    N'foreman@hospital.local',
                    N'patel@hospital.local',
                    N'rahman@hospital.local',
                    N'ionescu@hospital.local',
                    N'popescu@hospital.local');");
        }
    }
}
