using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOS.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberModuleAccess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberModuleAccesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    ModuleKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CanAccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberModuleAccesses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberModuleAccesses_HouseholdId_MemberId_ModuleKey",
                table: "MemberModuleAccesses",
                columns: new[] { "HouseholdId", "MemberId", "ModuleKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberModuleAccesses");
        }
    }
}
