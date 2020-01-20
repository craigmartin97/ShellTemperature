using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Models.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShellTemperatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Temperature = table.Column<double>(nullable: false),
                    RecordedDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShellTemperatures", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShellTemperatures");
        }
    }
}
