using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOS.Migrations
{
    /// <inheritdoc />
    public partial class AddReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TriggerAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecurrenceRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    SnoozedUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HouseholdId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    Visibility = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReminderRecipients",
                columns: table => new
                {
                    ReminderId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    NotifiedViaEmail = table.Column<bool>(type: "bit", nullable: false),
                    NotifiedInAppAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderRecipients", x => new { x.ReminderId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_ReminderRecipients_Reminders_ReminderId",
                        column: x => x.ReminderId,
                        principalTable: "Reminders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_HouseholdId",
                table: "Reminders",
                column: "HouseholdId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReminderRecipients");

            migrationBuilder.DropTable(
                name: "Reminders");
        }
    }
}
