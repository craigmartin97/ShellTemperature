using Microsoft.EntityFrameworkCore.Migrations;

namespace ShellTemperature.Models.Migrations
{
    public partial class AddGPSRecordings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Latitude",
                table: "ShellTemperatures",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Longitude",
                table: "ShellTemperatures",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ShellTemperatures");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ShellTemperatures");
        }
    }
}
