using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOS.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdId = table.Column<int>(type: "int", nullable: false),
                    ModuleKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleStates_HouseholdId_ModuleKey",
                table: "ModuleStates",
                columns: new[] { "HouseholdId", "ModuleKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleStates");
        }
    }
}
