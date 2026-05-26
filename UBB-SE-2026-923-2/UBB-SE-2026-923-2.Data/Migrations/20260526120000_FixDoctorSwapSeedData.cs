namespace UBB_SE_2026_923_2.Migrations
{
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Migrations;
    using UBB_SE_2026_923_2.Data;

    [DbContext(typeof(AppDbContext))]
    [Migration("20260526120000_FixDoctorSwapSeedData")]
    public partial class FixDoctorSwapSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 2)
                BEGIN
                    UPDATE dbo.Staff
                    SET Specialization = N'Diagnostician'
                    WHERE StaffID = 2;
                END

                IF EXISTS (SELECT 1 FROM dbo.Shifts WHERE Id IN (1, 2, 3, 4, 5))
                BEGIN
                    DECLARE @TomorrowStart DATETIME2 = DATEADD(DAY, 1, CAST(CAST(SYSDATETIME() AS DATE) AS DATETIME2));

                    UPDATE dbo.Shifts
                    SET Location = N'Clinic',
                        StartTime = DATEADD(HOUR, 9, @TomorrowStart),
                        EndTime = DATEADD(HOUR, 17, @TomorrowStart),
                        Status = N'SCHEDULED'
                    WHERE Id = 1;

                    UPDATE dbo.Shifts
                    SET Location = N'ER',
                        StartTime = DATEADD(HOUR, 18, @TomorrowStart),
                        EndTime = DATEADD(HOUR, 23, @TomorrowStart),
                        Status = N'SCHEDULED'
                    WHERE Id = 2;

                    UPDATE dbo.Shifts
                    SET Location = N'ER',
                        StartTime = DATEADD(HOUR, 9, @TomorrowStart),
                        EndTime = DATEADD(HOUR, 17, @TomorrowStart),
                        Status = N'SCHEDULED'
                    WHERE Id = 3;

                    UPDATE dbo.Shifts
                    SET Location = N'Pharmacy',
                        StartTime = DATEADD(HOUR, 9, @TomorrowStart),
                        EndTime = DATEADD(HOUR, 17, @TomorrowStart),
                        Status = N'SCHEDULED'
                    WHERE Id = 4;

                    UPDATE dbo.Shifts
                    SET Location = N'Pharmacy',
                        StartTime = DATEADD(HOUR, 18, @TomorrowStart),
                        EndTime = DATEADD(HOUR, 23, @TomorrowStart),
                        Status = N'SCHEDULED'
                    WHERE Id = 5;
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM dbo.Staff WHERE StaffID = 2)
                BEGIN
                    UPDATE dbo.Staff
                    SET Specialization = N'Oncology'
                    WHERE StaffID = 2;
                END");
        }
    }
}
