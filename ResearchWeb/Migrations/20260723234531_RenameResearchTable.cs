using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenameResearchTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Researches",
                table: "Researches");

            migrationBuilder.RenameTable(
                name: "Researches",
                newName: "الابحاث العلمية 2026");

            migrationBuilder.AddPrimaryKey(
                name: "PK_الابحاث العلمية 2026",
                table: "الابحاث العلمية 2026",
                column: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_الابحاث العلمية 2026",
                table: "الابحاث العلمية 2026");

            migrationBuilder.RenameTable(
                name: "الابحاث العلمية 2026",
                newName: "Researches");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Researches",
                table: "Researches",
                column: "ID");
        }
    }
}
