using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOS.Migrations
{
    /// <inheritdoc />
    public partial class AddItemShares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemShares", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemShares_Type_ItemId_MemberId",
                table: "ItemShares",
                columns: new[] { "Type", "ItemId", "MemberId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemShares");
        }
    }
}
