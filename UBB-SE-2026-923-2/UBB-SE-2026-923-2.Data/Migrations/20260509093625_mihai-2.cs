using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UBB_SE_2026_923_2.Migrations
{
    /// <inheritdoc />
    public partial class mihai2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserNotifications",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDiscounts",
                table: "UserDiscounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PharmacyHandovers",
                table: "PharmacyHandovers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeriodNotes",
                table: "PeriodNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemSubstances",
                table: "ItemSubstances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemBatches",
                table: "ItemBatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HangoutParticipants",
                table: "HangoutParticipants");

            migrationBuilder.DropColumn(
                name: "AssignedDoctorName",
                table: "ERRequests");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserDiscounts",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ShiftId",
                table: "ShiftSwapRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "RequesterId",
                table: "ShiftSwapRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ColleagueId",
                table: "ShiftSwapRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PharmacyHandovers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PeriodNotes",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ItemSubstances",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ItemBatches",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "HangoutParticipants",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Appointments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserNotifications",
                table: "UserNotifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDiscounts",
                table: "UserDiscounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PharmacyHandovers",
                table: "PharmacyHandovers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeriodNotes",
                table: "PeriodNotes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemSubstances",
                table: "ItemSubstances",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemBatches",
                table: "ItemBatches",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HangoutParticipants",
                table: "HangoutParticipants",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDiscounts_UserId",
                table: "UserDiscounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyHandovers_PharmacistId",
                table: "PharmacyHandovers",
                column: "PharmacistId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodNotes_UserId",
                table: "PeriodNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemSubstances_ItemId",
                table: "ItemSubstances",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemBatches_ItemId",
                table: "ItemBatches",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_HangoutParticipants_HangoutId",
                table: "HangoutParticipants",
                column: "HangoutId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserNotifications",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_UserId",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDiscounts",
                table: "UserDiscounts");

            migrationBuilder.DropIndex(
                name: "IX_UserDiscounts_UserId",
                table: "UserDiscounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PharmacyHandovers",
                table: "PharmacyHandovers");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyHandovers_PharmacistId",
                table: "PharmacyHandovers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeriodNotes",
                table: "PeriodNotes");

            migrationBuilder.DropIndex(
                name: "IX_PeriodNotes_UserId",
                table: "PeriodNotes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemSubstances",
                table: "ItemSubstances");

            migrationBuilder.DropIndex(
                name: "IX_ItemSubstances_ItemId",
                table: "ItemSubstances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemBatches",
                table: "ItemBatches");

            migrationBuilder.DropIndex(
                name: "IX_ItemBatches_ItemId",
                table: "ItemBatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HangoutParticipants",
                table: "HangoutParticipants");

            migrationBuilder.DropIndex(
                name: "IX_HangoutParticipants_HangoutId",
                table: "HangoutParticipants");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserNotifications");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserDiscounts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PharmacyHandovers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PeriodNotes");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ItemSubstances");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ItemBatches");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "HangoutParticipants");

            migrationBuilder.AlterColumn<int>(
                name: "ShiftId",
                table: "ShiftSwapRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RequesterId",
                table: "ShiftSwapRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ColleagueId",
                table: "ShiftSwapRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedDoctorName",
                table: "ERRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Appointments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserNotifications",
                table: "UserNotifications",
                columns: new[] { "UserId", "ItemId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDiscounts",
                table: "UserDiscounts",
                columns: new[] { "UserId", "ItemId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PharmacyHandovers",
                table: "PharmacyHandovers",
                columns: new[] { "PharmacistId", "HandoverDate" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeriodNotes",
                table: "PeriodNotes",
                columns: new[] { "UserId", "NoteId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                columns: new[] { "OrderId", "ItemId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemSubstances",
                table: "ItemSubstances",
                columns: new[] { "ItemId", "SubstanceName" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemBatches",
                table: "ItemBatches",
                columns: new[] { "ItemId", "ExpirationDate" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_HangoutParticipants",
                table: "HangoutParticipants",
                columns: new[] { "HangoutId", "StaffId" });
        }
    }
}
