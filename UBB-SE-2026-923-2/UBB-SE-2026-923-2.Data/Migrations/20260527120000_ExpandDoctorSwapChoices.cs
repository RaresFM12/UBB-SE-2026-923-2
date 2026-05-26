namespace UBB_SE_2026_923_2.Migrations
{
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;
    using UBB_SE_2026_923_2.Data;

    [DbContext(typeof(AppDbContext))]
    [Migration("20260527120000_ExpandDoctorSwapChoices")]
    public partial class ExpandDoctorSwapChoices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT dbo.Staff ON;

                IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 6)
                    INSERT INTO dbo.Staff (StaffID, Email, PasswordHash, Role, Department, FirstName, LastName, ContactInfo, Available, LicenseNumber, Specialization, Status, Certification, YearsOfExperience, HourlyRate)
                    VALUES (6, N'cameron@hospital.local', N'hash', N'Doctor', N'Diagnostics', N'Allison', N'Cameron', N'555-0106', 1, N'LIC-1006', N'Diagnostician', N'AVAILABLE', N'Board Certified', 7, 135.0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 7)
                    INSERT INTO dbo.Staff (StaffID, Email, PasswordHash, Role, Department, FirstName, LastName, ContactInfo, Available, LicenseNumber, Specialization, Status, Certification, YearsOfExperience, HourlyRate)
                    VALUES (7, N'chase@hospital.local', N'hash', N'Doctor', N'Diagnostics', N'Robert', N'Chase', N'555-0107', 1, N'LIC-1007', N'Diagnostician', N'AVAILABLE', N'Board Certified', 9, 145.0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 8)
                    INSERT INTO dbo.Staff (StaffID, Email, PasswordHash, Role, Department, FirstName, LastName, ContactInfo, Available, LicenseNumber, Specialization, Status, Certification, YearsOfExperience, HourlyRate)
                    VALUES (8, N'foreman@hospital.local', N'hash', N'Doctor', N'Diagnostics', N'Eric', N'Foreman', N'555-0108', 1, N'LIC-1008', N'Diagnostician', N'AVAILABLE', N'Board Certified', 11, 155.0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 9)
                    INSERT INTO dbo.Staff (StaffID, Email, PasswordHash, Role, Department, FirstName, LastName, ContactInfo, Available, LicenseNumber, Specialization, Status, Certification, YearsOfExperience, HourlyRate)
                    VALUES (9, N'patel@hospital.local', N'hash', N'Doctor', N'Surgery', N'Nina', N'Patel', N'555-0109', 1, N'LIC-1009', N'Surgery', N'AVAILABLE', N'Board Certified', 6, 130.0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 10)
                    INSERT INTO dbo.Staff (StaffID, Email, PasswordHash, Role, Department, FirstName, LastName, ContactInfo, Available, LicenseNumber, Specialization, Status, Certification, YearsOfExperience, HourlyRate)
                    VALUES (10, N'rahman@hospital.local', N'hash', N'Doctor', N'Surgery', N'Omar', N'Rahman', N'555-0110', 1, N'LIC-1010', N'Surgery', N'AVAILABLE', N'Board Certified', 10, 150.0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 11)
                    INSERT INTO dbo.Staff (StaffID, Email, PasswordHash, Role, Department, FirstName, LastName, ContactInfo, Available, LicenseNumber, Specialization, Status, Certification, YearsOfExperience, HourlyRate)
                    VALUES (11, N'ionescu@hospital.local', N'hash', N'Doctor', N'Cardiology', N'Elena', N'Ionescu', N'555-0111', 1, N'LIC-1011', N'Cardiology', N'AVAILABLE', N'Board Certified', 5, 125.0);

                IF NOT EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 12)
                    INSERT INTO dbo.Staff (StaffID, Email, PasswordHash, Role, Department, FirstName, LastName, ContactInfo, Available, LicenseNumber, Specialization, Status, Certification, YearsOfExperience, HourlyRate)
                    VALUES (12, N'popescu@hospital.local', N'hash', N'Doctor', N'Cardiology', N'Victor', N'Popescu', N'555-0112', 1, N'LIC-1012', N'Cardiology', N'AVAILABLE', N'Board Certified', 8, 140.0);

                SET IDENTITY_INSERT dbo.Staff OFF;

                DECLARE @TomorrowStart DATETIME2 = DATEADD(DAY, 1, CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2));

                IF NOT EXISTS (SELECT 1 FROM dbo.Shifts WHERE Location = N'Diagnostics North')
                BEGIN
                    INSERT INTO dbo.Shifts (StaffId, Location, StartTime, EndTime, Status) VALUES
                    (1, N'Diagnostics North', DATEADD(DAY, 2, DATEADD(HOUR, 8, @TomorrowStart)), DATEADD(DAY, 2, DATEADD(HOUR, 14, @TomorrowStart)), N'SCHEDULED'),
                    (1, N'Clinic East', DATEADD(DAY, 3, DATEADD(HOUR, 14, @TomorrowStart)), DATEADD(DAY, 3, DATEADD(HOUR, 22, @TomorrowStart)), N'SCHEDULED'),
                    (1, N'Clinic West', DATEADD(DAY, 6, DATEADD(HOUR, 9, @TomorrowStart)), DATEADD(DAY, 6, DATEADD(HOUR, 17, @TomorrowStart)), N'SCHEDULED'),
                    (2, N'ER West', DATEADD(DAY, 2, DATEADD(HOUR, 16, @TomorrowStart)), DATEADD(DAY, 2, DATEADD(HOUR, 23, @TomorrowStart)), N'SCHEDULED'),
                    (2, N'Clinic South', DATEADD(DAY, 4, DATEADD(HOUR, 8, @TomorrowStart)), DATEADD(DAY, 4, DATEADD(HOUR, 16, @TomorrowStart)), N'SCHEDULED'),
                    (2, N'Diagnostics West', DATEADD(DAY, 7, DATEADD(HOUR, 12, @TomorrowStart)), DATEADD(DAY, 7, DATEADD(HOUR, 20, @TomorrowStart)), N'SCHEDULED'),
                    (3, N'Surgery A', DATEADD(DAY, 2, DATEADD(HOUR, 12, @TomorrowStart)), DATEADD(DAY, 2, DATEADD(HOUR, 20, @TomorrowStart)), N'SCHEDULED'),
                    (3, N'Surgery B', DATEADD(DAY, 5, DATEADD(HOUR, 8, @TomorrowStart)), DATEADD(DAY, 5, DATEADD(HOUR, 16, @TomorrowStart)), N'SCHEDULED'),
                    (6, N'Diagnostics North', DATEADD(HOUR, 18, @TomorrowStart), DATEADD(HOUR, 23, @TomorrowStart), N'SCHEDULED'),
                    (6, N'Diagnostics South', DATEADD(DAY, 2, DATEADD(HOUR, 9, @TomorrowStart)), DATEADD(DAY, 2, DATEADD(HOUR, 13, @TomorrowStart)), N'SCHEDULED'),
                    (6, N'Clinic East', DATEADD(DAY, 4, DATEADD(HOUR, 14, @TomorrowStart)), DATEADD(DAY, 4, DATEADD(HOUR, 22, @TomorrowStart)), N'SCHEDULED'),
                    (7, N'Clinic North', DATEADD(DAY, 2, DATEADD(HOUR, 14, @TomorrowStart)), DATEADD(DAY, 2, DATEADD(HOUR, 22, @TomorrowStart)), N'SCHEDULED'),
                    (7, N'Diagnostics West', DATEADD(DAY, 3, DATEADD(HOUR, 8, @TomorrowStart)), DATEADD(DAY, 3, DATEADD(HOUR, 16, @TomorrowStart)), N'SCHEDULED'),
                    (7, N'ER East', DATEADD(DAY, 6, DATEADD(HOUR, 18, @TomorrowStart)), DATEADD(DAY, 6, DATEADD(HOUR, 23, @TomorrowStart)), N'SCHEDULED'),
                    (8, N'Diagnostics East', DATEADD(DAY, 3, DATEADD(HOUR, 18, @TomorrowStart)), DATEADD(DAY, 3, DATEADD(HOUR, 23, @TomorrowStart)), N'SCHEDULED'),
                    (8, N'Clinic South', DATEADD(DAY, 5, DATEADD(HOUR, 9, @TomorrowStart)), DATEADD(DAY, 5, DATEADD(HOUR, 17, @TomorrowStart)), N'SCHEDULED'),
                    (8, N'ER North', DATEADD(DAY, 7, DATEADD(HOUR, 8, @TomorrowStart)), DATEADD(DAY, 7, DATEADD(HOUR, 14, @TomorrowStart)), N'SCHEDULED'),
                    (9, N'Surgery A', DATEADD(HOUR, 18, @TomorrowStart), DATEADD(HOUR, 23, @TomorrowStart), N'SCHEDULED'),
                    (9, N'Surgery C', DATEADD(DAY, 2, DATEADD(HOUR, 8, @TomorrowStart)), DATEADD(DAY, 2, DATEADD(HOUR, 16, @TomorrowStart)), N'SCHEDULED'),
                    (10, N'Surgery B', DATEADD(DAY, 2, DATEADD(HOUR, 18, @TomorrowStart)), DATEADD(DAY, 2, DATEADD(HOUR, 23, @TomorrowStart)), N'SCHEDULED'),
                    (10, N'Surgery D', DATEADD(DAY, 4, DATEADD(HOUR, 9, @TomorrowStart)), DATEADD(DAY, 4, DATEADD(HOUR, 17, @TomorrowStart)), N'SCHEDULED'),
                    (11, N'Cardiology A', DATEADD(HOUR, 9, @TomorrowStart), DATEADD(HOUR, 17, @TomorrowStart), N'SCHEDULED'),
                    (11, N'Cardiology C', DATEADD(DAY, 3, DATEADD(HOUR, 18, @TomorrowStart)), DATEADD(DAY, 3, DATEADD(HOUR, 23, @TomorrowStart)), N'SCHEDULED'),
                    (12, N'Cardiology B', DATEADD(HOUR, 18, @TomorrowStart), DATEADD(HOUR, 23, @TomorrowStart), N'SCHEDULED'),
                    (12, N'Cardiology D', DATEADD(DAY, 4, DATEADD(HOUR, 9, @TomorrowStart)), DATEADD(DAY, 4, DATEADD(HOUR, 17, @TomorrowStart)), N'SCHEDULED');
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM dbo.Shifts
                WHERE Location IN (
                    N'Diagnostics North',
                    N'Clinic East',
                    N'Clinic West',
                    N'ER West',
                    N'Clinic South',
                    N'Diagnostics West',
                    N'Surgery A',
                    N'Surgery B',
                    N'Diagnostics South',
                    N'Clinic North',
                    N'ER East',
                    N'Diagnostics East',
                    N'ER North',
                    N'Surgery C',
                    N'Surgery D',
                    N'Cardiology A',
                    N'Cardiology B',
                    N'Cardiology C',
                    N'Cardiology D');

                DELETE FROM dbo.Staff WHERE StaffID IN (6, 7, 8, 9, 10, 11, 12);");
        }
    }
}
