using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchWeb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Researches",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    اسم_الباحث = table.Column<string>(type: "TEXT", nullable: true),
                    تاريخ_الاجتماع = table.Column<DateTime>(type: "TEXT", nullable: true),
                    عنوان_البحث = table.Column<string>(type: "TEXT", nullable: true),
                    رقم_البحث = table.Column<string>(type: "TEXT", nullable: true),
                    رقم_الاجتماع = table.Column<string>(type: "TEXT", nullable: true),
                    نتيجة_البحث = table.Column<string>(type: "TEXT", nullable: true),
                    رقم_الهاتف = table.Column<string>(type: "TEXT", nullable: true),
                    توصيات_اللجنة = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Researches", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    FullName = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Researches");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
